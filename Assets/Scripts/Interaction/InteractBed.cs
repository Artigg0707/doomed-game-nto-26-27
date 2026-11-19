using System;
using System.Collections;
using UnityEngine;

public class InteractBed : MonoBehaviour, IInteractable, IHighlightable
{
    [Header("Ссылки")]
    [SerializeField] private MorningReportPanel morningReportPanel;
    [SerializeField] private CanvasGroup fadePanel; // чёрная панель на Canvas для fade

    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1f;

    private Material _outlineMaterial;
    private bool _isSleeping = false;
    private NightReportData _pendingReport;

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
    }

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

    public void Interact()
    {
        if (_isSleeping || morningReportPanel == null)
            return;

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