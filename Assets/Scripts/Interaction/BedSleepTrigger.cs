using System;
using System.Collections;
using UnityEngine;

public class BedSleepTrigger : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private MorningReportPanel morningReportPanel;
    [SerializeField] private CanvasGroup fadePanel;

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1f;

    private bool _isSleeping = false;
    private NightReportData _pendingReport;

    private void OnEnable()
    {
        if (DayCycleManager.Instance != null)
            DayCycleManager.Instance.OnNightComplete += OnNightComplete;
    }

    private void OnDisable()
    {
        if (DayCycleManager.Instance != null)
            DayCycleManager.Instance.OnNightComplete -= OnNightComplete;
    }

    public void OnSleepButtonClicked()
    {
        if (_isSleeping)
        {
            Debug.LogWarning("[BedSleepTrigger] Уже идёт процесс сна, игнорируем повторный клик.");
            return;
        }

        Debug.Log("[BedSleepTrigger] Нажата кнопка Спать, начинаем последовательность сна.");
        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        _isSleeping = true;

        // Блокируем управление игроком
        var playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
            Debug.Log("[BedSleepTrigger] PlayerMove отключён.");
        }

        var interactor = FindObjectOfType<Interactor>();
        if (interactor != null)
        {
            interactor.enabled = false;
            Debug.Log("[BedSleepTrigger] Interactor отключён.");
        }

        // Fade to black (если есть панель затемнения)
        if (fadePanel != null)
        {
            Debug.Log("[BedSleepTrigger] Fade to black...");
            fadePanel.blocksRaycasts = true;
            fadePanel.interactable = true;

            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime / fadeDuration;
                fadePanel.alpha = Mathf.Lerp(0, 1, t);
                yield return null;
            }
            fadePanel.alpha = 1f;
            Debug.Log("[BedSleepTrigger] Экран полностью чёрный.");
        }
        else
        {
            Debug.LogWarning("[BedSleepTrigger] FadePanel не назначен! Переход будет мгновенным.");
        }

        // Запускаем ночной расчёт
        Debug.Log("[BedSleepTrigger] Вызов DayCycleManager.StartNextDay()...");
        DayCycleManager.Instance.StartNextDay();

        yield return new WaitForSeconds(0.3f);

        // Показываем панель утренней сводки (всегда, даже если отчёт пустой)
        if (morningReportPanel != null)
        {
            // Если отчёт не пришёл через событие, создаём минимальный
            if (_pendingReport == null)
            {
                Debug.LogWarning("[BedSleepTrigger] Отчёт ночи не получен через событие, создаём базовый.");
                _pendingReport = new NightReportData
                {
                    completedDay = DayCycleManager.Instance.CurrentDay - 1,
                    newDay = DayCycleManager.Instance.CurrentDay,
                    fuelConsumed = false,
                    generatorWorked = false,
                    temperatureBefore = ResourceManager.Instance?.TemperatureLevel ?? 3,
                    temperatureAfter = ResourceManager.Instance?.TemperatureLevel ?? 3
                };
            }

            Debug.Log("[BedSleepTrigger] Показываем панель утренней сводки.");
            morningReportPanel.Show(_pendingReport, OnSummaryContinued);
        }
        else
        {
            Debug.LogError("[BedSleepTrigger] MorningReportPanel не назначен!");
            OnSummaryContinued();
        }
    }

    private void OnNightComplete(NightReportData report)
    {
        _pendingReport = report;
        Debug.Log($"[BedSleepTrigger] Получен отчёт ночи. День: {report.newDay}, Событий: {report.HasAnyEvents}");
    }

    private void OnSummaryContinued()
    {
        Debug.Log("[BedSleepTrigger] Начало Fade In (возврат к игре).");

        if (fadePanel != null)
        {
            StartCoroutine(FadeInRoutine());
        }
        else
        {
            FinishSleep();
        }
    }

    private IEnumerator FadeInRoutine()
    {
        Debug.Log("[BedSleepTrigger] Fade In запущен.");
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime / fadeDuration;
            fadePanel.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        fadePanel.alpha = 0f;
        Debug.Log("[BedSleepTrigger] Экран полностью прозрачный.");
        FinishSleep();
    }

    private void FinishSleep()
    {
        Debug.Log("[BedSleepTrigger] Завершение сна, возврат управления.");

        morningReportPanel?.Hide();

        var playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = true;
            Debug.Log("[BedSleepTrigger] PlayerMove включён.");
        }

        var interactor = FindObjectOfType<Interactor>();
        if (interactor != null)
        {
            interactor.enabled = true;
            Debug.Log("[BedSleepTrigger] Interactor включён.");
        }

        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = false;
            fadePanel.interactable = false;
        }

        _isSleeping = false;
        _pendingReport = null;
    }

    /// <summary>
    /// ��������� �����, ���� �������� �� ������ ������. 
    /// ����� ������� ����� ������� Unity: BedSleepTrigger.ForceReset()
    /// </summary>
    [ContextMenu("Force Reset Sleep (экстренный выход из чёрного экрана)")]
    public static void ForceReset()
    {
        var trigger = FindObjectOfType<BedSleepTrigger>();
        if (trigger != null)
        {
            trigger.FinishSleep();
            Debug.Log("[BedSleepTrigger] Экстренный сброс выполнен.");
        }

        // Также сбрасываем все CanvasGroup на сцене на всякий случай
        var groups = FindObjectsOfType<CanvasGroup>();
        foreach (var g in groups)
        {
            g.alpha = 0;
            g.blocksRaycasts = false;
            g.interactable = false;
        }
    }
}
