using System;
using System.Collections;
using UnityEngine;

public class InteractBed : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Ссылки")]
    [SerializeField] private MorningReportPanel morningReportPanel;
    [SerializeField] private DefeatScreenPanel defeatScreenPanel;
    [SerializeField] private CanvasGroup fadePanel; // чёрная панель на Canvas для fade

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1f;

    private Material _outlineMaterial;
    private bool _isSleeping = false;
    private NightReportData _pendingReport;
    private bool _defeatOccurred;
    private bool _victoryOccurred;

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            var mats = renderer.materials;
            _outlineMaterial = mats.Length > 1 ? mats[1] : mats.Length == 1 ? mats[0] : null;
        }

        if (fadePanel != null)
            fadePanel.alpha = 0f;

        SubscribeToCycleEvents();
    }

    private void OnEnable()
    {
        SubscribeToCycleEvents();
    }

    private void OnDisable()
    {
        if (DayCycleManager.Instance != null)
        {
            DayCycleManager.Instance.OnNightComplete -= OnNightComplete;
            DayCycleManager.Instance.OnDefeat -= OnDefeat;
            DayCycleManager.Instance.OnVictory -= OnVictory;
        }
    }

    private void SubscribeToCycleEvents()
    {
        if (DayCycleManager.Instance == null)
            return;

        DayCycleManager.Instance.OnNightComplete -= OnNightComplete;
        DayCycleManager.Instance.OnNightComplete += OnNightComplete;
        DayCycleManager.Instance.OnDefeat -= OnDefeat;
        DayCycleManager.Instance.OnDefeat += OnDefeat;
        DayCycleManager.Instance.OnVictory -= OnVictory;
        DayCycleManager.Instance.OnVictory += OnVictory;
    }

    public void Interact()
    {
        if (_isSleeping || morningReportPanel == null)
            return;

        _defeatOccurred = false;
        _victoryOccurred = false;
        StartCoroutine(SleepSequence());
    }

    private IEnumerator SleepSequence()
    {
        _isSleeping = true;

        // Отключаем управление игроком
        var playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove != null)
            playerMove.enabled = false;

        var interactor = FindObjectOfType<Interactor>();
        if (interactor != null)
            interactor.enabled = false;

        // Fade to black
        if (fadePanel != null)
        {
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime / fadeDuration;
                fadePanel.alpha = Mathf.Lerp(0, 1, t);
                yield return null;
            }
            fadePanel.alpha = 1f;
        }

        // Запускаем ночной расчёт (вызовет OnNightComplete синхронно)
        DayCycleManager.Instance.StartNextDay();

        // Небольшая пауза, чтобы отчет успел сформироваться
        yield return new WaitForSeconds(0.3f);

        if (_defeatOccurred ||
            (DayCycleManager.Instance != null &&
             DayCycleManager.Instance.IsDefeat()))
        {
            DefeatScreenPanel panel = defeatScreenPanel != null
                ? defeatScreenPanel
                : DefeatScreenPanel.Instance;

            if (panel != null)
                panel.Show();
            else
                Debug.LogError("DefeatScreenPanel не найден.", this);

            yield break;
        }

        if (_victoryOccurred ||
            (DayCycleManager.Instance != null &&
             DayCycleManager.Instance.IsVictory()))
        {
            DefeatScreenPanel panel = defeatScreenPanel != null
                ? defeatScreenPanel
                : DefeatScreenPanel.Instance;

            if (panel != null)
                panel.ShowVictory();
            else
                Debug.LogError("DefeatScreenPanel не найден.", this);

            yield break;
        }

        // Показываем панель утренней сводки (она уже получила данные через событие)
        if (_pendingReport != null)
        {
            morningReportPanel.Show(_pendingReport, OnSummaryContinued);
        }
        else
        {
            // Если события не было — просто продолжаем
            OnSummaryContinued();
        }
    }

    private void OnNightComplete(NightReportData report)
    {
        _pendingReport = report;
    }

    private void OnDefeat()
    {
        _defeatOccurred = true;
    }

    private void OnVictory()
    {
        _victoryOccurred = true;
    }

    private void OnSummaryContinued()
    {
        // Fade in
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
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime / fadeDuration;
            fadePanel.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        fadePanel.alpha = 0f;
        FinishSleep();
    }

    private void FinishSleep()
    {
        morningReportPanel.Hide();

        var playerMove = FindObjectOfType<PlayerMove>();
        if (playerMove != null)
            playerMove.enabled = true;

        var interactor = FindObjectOfType<Interactor>();
        if (interactor != null)
            interactor.enabled = true;

        _isSleeping = false;
        _pendingReport = null;
    }

    public void Highlight(bool state)
    {
        if (_isSleeping) return;
        float target = state ? 1.1f : 0f;
        if (_outlineMaterial != null)
            _outlineMaterial.SetFloat("_OutlineSize", target);
    }
}