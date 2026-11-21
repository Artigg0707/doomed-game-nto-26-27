using System;
using System.Collections;
using UnityEngine;

public class BedSleepTrigger : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private MorningReportPanel morningReportPanel;
    [SerializeField] private DefeatScreenPanel defeatScreenPanel;
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private CharacterManager characterManager;

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

        if (DayCycleManager.Instance != null &&
            DayCycleManager.Instance.IsDefeat())
        {
            if (defeatScreenPanel != null)
            {
                defeatScreenPanel.Show();
            }
            else
            {
                Debug.LogError(
                    "DefeatScreenPanel не назначен на BedSleepTrigger.",
                    this);
            }

            yield break;
        }

        // Показываем панель утренней сводки (всегда, даже если отчёт пустой)
        if (morningReportPanel != null)
        {
            // Если отчёт не пришёл через событие, собираем данные напрямую
            if (_pendingReport == null)
            {
                Debug.LogWarning("[BedSleepTrigger] Отчёт ночи не получен через событие, собираем данные напрямую.");

                ResourceManager resources = ResourceManager.Instance;
                bool fuelConsumed = false; // мы не знаем, было ли израсходовано, если событие не пришло
                int tempBefore = resources != null ? resources.TemperatureLevel : 3;
                int tempAfter = resources != null ? resources.TemperatureLevel : 3;

                _pendingReport = new NightReportData
                {
                    completedDay = DayCycleManager.Instance.CurrentDay - 1,
                    newDay = DayCycleManager.Instance.CurrentDay,
                    fuelConsumed = fuelConsumed,
                    generatorWorked = false,
                    temperatureBefore = tempBefore,
                    temperatureAfter = tempAfter
                };

                // Добавляем статус каждого жителя из CharacterManager
                if (characterManager != null)
                {
                    foreach (GeneratedCharacter c in characterManager.characters)
                    {
                        if (c == null || c.isDead || c.isExiled)
                            continue;

                        string foodState = c.daysWithoutFood == 0 ? "сыт" : $"без еды {c.daysWithoutFood} дн.";
                        string waterState = c.daysWithoutWater == 0 ? "напоен" : $"без воды {c.daysWithoutWater} дн.";
                        string healthState = c.health >= 75 ? "здоров" : (c.health >= 35 ? "ранен" : "при смерти");
                        _pendingReport.survivalChanges.Add($"{c.fullName}: {foodState}, {waterState}, {c.health} HP, мораль {c.morale} ({healthState}).");

                        if (c.daysWithoutFood == 1)
                            _pendingReport.survivalChanges.Add($"  ↳ мораль −10 (голод 1 день)");
                        else if (c.daysWithoutFood >= 2)
                            _pendingReport.survivalChanges.Add($"  ↳ Истощен! −25 HP, −20 мораль.");

                        if (c.daysWithoutWater == 1)
                            _pendingReport.survivalChanges.Add($"  ↳ мораль −15 (жажда 1 день)");
                        else if (c.daysWithoutWater >= 2)
                            _pendingReport.survivalChanges.Add($"  ↳ Обезвоживание! −40 HP.");
                    }

                    if (resources != null)
                    {
                        if (fuelConsumed)
                            _pendingReport.resourceChanges.Add("Топлива −1 канистра (обогрев работал).");
                        else
                            _pendingReport.resourceChanges.Add("Топлива не хватило. Обогрев отключён.");

                        _pendingReport.resourceChanges.Add($"Еда на складе: {resources.FoodCans} банок.");
                        _pendingReport.resourceChanges.Add($"Вода на складе: {resources.WaterBottles} бутылок.");
                        _pendingReport.resourceChanges.Add($"Температура: {resources.TemperatureLevel}/3.");
                    }
                }
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
