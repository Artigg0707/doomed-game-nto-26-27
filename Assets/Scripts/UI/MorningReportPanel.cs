using System;
using System.Text;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class MorningReportPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;

    [Header("Связанные объекты (назначьте в инспекторе)")]
    [Tooltip("Объект меню кровати (Canvas 'Кровать кнавас'), который нужно скрыть после сводки")]
    [SerializeField] private GameObject bedMenuPanel;
    [Tooltip("Базовая камера игрока (CinemachineCamera), приоритет которой вернуть после сна")]
    [SerializeField] private CinemachineCamera playerCamera;
    [Tooltip("Базовый приоритет камеры игрока (обычно 10 или выше)")]
    [SerializeField] private int playerCameraBasePriority = 10;

    private Action _onContinue;

    private void Awake()
    {
        gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }

    public void Show(NightReportData report, Action onContinue)
    {
        _onContinue = onContinue;
        gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = $"День {report.newDay} — Утренняя сводка";

        if (bodyText != null)
            bodyText.text = BuildReportText(report);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnContinueClicked()
    {
        Hide();

        // Закрываем меню кровати
        if (bedMenuPanel != null)
            bedMenuPanel.SetActive(false);

        // Возвращаем приоритет базовой камере игрока
        if (playerCamera != null)
            playerCamera.Priority = playerCameraBasePriority;

        _onContinue?.Invoke();
        _onContinue = null;
    }

    private string BuildReportText(NightReportData r)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"День {r.completedDay} закончен. Наступает день {r.newDay}.");
        sb.AppendLine();

        // Состояние бункера
        if (r.fuelConsumed)
            sb.AppendLine("Генератор проработал всю ночь. Потрачено 1 канистру топлива.");
        else
            sb.AppendLine("Топлива не хватило. Генератор отключён, бункер замерзает.");

        sb.AppendLine($"Температура в бункере: {r.temperatureBefore} → {r.temperatureAfter}");
        sb.AppendLine();

        // Ресурсы на складе (всегда показываем)
        sb.AppendLine("Запасы на складе:");
        if (r.resourceChanges.Count > 0)
        {
            foreach (var m in r.resourceChanges)
                sb.AppendLine("► " + m);
        }
        else
        {
            sb.AppendLine("► Без изменений.");
        }

        sb.AppendLine();

        // Состояние жителей (всегда показываем)
        sb.AppendLine("Состояние жителей:");
        if (r.survivalChanges.Count > 0)
        {
            foreach (var m in r.survivalChanges)
                sb.AppendLine("► " + m);
        }
        else
        {
            sb.AppendLine("► Все живы и здоровы, никто не пострадал.");
        }

        // Смерти
        foreach (var d in r.deaths)
            sb.AppendLine($"► {d} не выжил.");

        sb.AppendLine();

        // Итоговая строка
        sb.AppendLine(r.GetSummaryLine());

        return sb.ToString();
    }
}