using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [Header("Настройки комментариев коменданта")]
    [SerializeField] private CommandantCommentSettings commentSettings;

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

        // Состояние жителей (всегда показываем всех)
        sb.AppendLine("Состояние жителей:");
        if (r.survivalChanges.Count > 0)
        {
            foreach (var m in r.survivalChanges)
                sb.AppendLine("► " + m);
        }
        else
        {
            sb.AppendLine("► Нет жителей в бункере.");
        }

        // Смерти
        foreach (var d in r.deaths)
            sb.AppendLine($"► {d} не выжил.");

        // Уходы из бункера
        foreach (var d in r.departures)
            sb.AppendLine($"► {d} ушёл из бункера, не выдержав происходящего.");

        // Возвращение Expedition
        if (r.expeditionReturns.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Экспедиции:");
            foreach (var e in r.expeditionReturns)
                sb.AppendLine($"► {e}");
        }

        sb.AppendLine();

        // Итоговая строка
        sb.AppendLine(r.GetSummaryLine());

        sb.AppendLine();
        sb.AppendLine("Заметка коменданта:");
        sb.AppendLine("► " + GetCommandantComment(r));

        return sb.ToString();
    }

    private string GetCommandantComment(NightReportData r)
    {
        var hungry = new List<string>();
        var thirsty = new List<string>();
        var injured = new List<string>();
        var dead = new List<string>(r.deaths);
        var departed = new List<string>(r.departures);

        foreach (var line in r.survivalChanges)
        {
            if (line.Contains("без еды"))
            {
                var name = line.Split(':')[0].Trim();
                hungry.Add(name);
            }
            if (line.Contains("без воды"))
            {
                var name = line.Split(':')[0].Trim();
                thirsty.Add(name);
            }
            if (line.Contains("HP,") && line.Contains("ранен"))
            {
                var name = line.Split(':')[0].Trim();
                injured.Add(name);
            }
        }

        if (commentSettings != null)
        {
            if (dead.Count > 0)
                return FormatRandom(commentSettings.deathComments, string.Join(", ", dead), string.Join(", ", dead));

            if (departed.Count > 0)
                return $"После этой ночи {string.Join(", ", departed)} ушёл из бункера. Мораль больше не выдержала.";

            if (hungry.Count >= 2)
                return FormatRandom(commentSettings.hungryComments, hungry[0], string.Join(", ", hungry));

            if (thirsty.Count >= 2)
                return FormatRandom(commentSettings.thirstyComments, thirsty[0], string.Join(", ", thirsty));

            if (injured.Count > 0)
                return FormatRandom(commentSettings.injuredComments, injured[0], string.Join(", ", injured));

            if (hungry.Count == 1 && thirsty.Count == 1)
                return FormatRandom(commentSettings.hungryAndThirstyComments, hungry[0], thirsty[0]);

            if (hungry.Count == 1 || thirsty.Count == 1 || injured.Count == 1)
            {
                var name = hungry.Count > 0 ? hungry[0] : thirsty.Count > 0 ? thirsty[0] : injured[0];
                return FormatRandom(commentSettings.loneComments, name);
            }

            return FormatRandom(commentSettings.calmComments);
        }

        if (dead.Count > 0)
            return $"Вчера ушли {string.Join(", ", dead)}. Остальные выглядят так, будто сами скоро уйдут.";
        if (departed.Count > 0)
            return $"{string.Join(", ", departed)} ушёл из бункера. Иногда человек уходит раньше, чем умирает.";
        if (hungry.Count >= 2)
            return $"Сегодня в бункере взгляды слишком голодные. Лучше не оставлять {hungry[0]} одного с консервами.";
        if (thirsty.Count >= 2)
            return $"Вчерашняя ночь была сухой. {thirsty[0]} уже смотрит на канистру с тоской в глазах.";
        if (injured.Count > 0)
            return $"{injured[0]} хромает и хмурится. Будем надеяться, что до утра доживёт.";
        if (hungry.Count == 1 && thirsty.Count == 1)
            return $"{hungry[0]} голоден, а {thirsty[0]} хочет пить. Двоим было бы легче, но у нас что есть, что есть.";
        if (hungry.Count == 1 || thirsty.Count == 1 || injured.Count == 1)
        {
            var name = hungry.Count > 0 ? hungry[0] : thirsty.Count > 0 ? thirsty[0] : injured[0];
            return $"{name} выглядит так, будто сегодня ночью видел что-то странное в вентиляции.";
        }

        return "Все на месте. Надеюсь, завтра будет не хуже.";
    }

    private string FormatRandom(List<string> templates, params string[] args)
    {
        if (templates == null || templates.Count == 0)
            return "Все на месте. Надеюсь, завтра будет не хуже.";

        var template = templates[UnityEngine.Random.Range(0, templates.Count)];
        var name = args.Length > 0 ? args[0] : "";
        var names = args.Length > 1 ? args[1] : name;

        return template
            .Replace("{name}", name)
            .Replace("{names}", names)
            .Replace("{name1}", name)
            .Replace("{name2}", args.Length > 1 ? args[1] : "");
    }
}
