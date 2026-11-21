using TMPro;
using UnityEngine;

public class ActiveExpeditionsUI : MonoBehaviour
{
    [SerializeField] private ExpeditionManager expeditionManager;
    [SerializeField] private Transform entriesContent;
    [SerializeField] private ActiveExpeditionEntryUI entryPrefab;
    [SerializeField] private TMP_Text emptyText;
    [SerializeField] private TMP_Text countText;

    private void OnEnable()
    {
        if (expeditionManager != null)
        {
            expeditionManager.OnExpeditionsChanged -= Refresh;
            expeditionManager.OnExpeditionsChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (expeditionManager != null)
            expeditionManager.OnExpeditionsChanged -= Refresh;
    }

    public void Refresh()
    {
        if (entriesContent == null)
        {
            Debug.LogError(
                "В ActiveExpeditionsUI не назначен Entries Content.",
                this);
            return;
        }

        if (expeditionManager == null)
        {
            Debug.LogError(
                "В ActiveExpeditionsUI не назначен Expedition Manager.",
                this);
            return;
        }

        if (entryPrefab == null)
        {
            Debug.LogError(
                "В ActiveExpeditionsUI не назначен Entry Prefab.",
                this);
            return;
        }

        ClearEntries();

        int count = expeditionManager.ActiveExpeditionCount;

        if (countText != null)
        {
            countText.text =
                $"Активные вылазки: {count}/" +
                $"{expeditionManager.MaxConcurrentExpeditions}";
        }

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(count == 0);

            if (count == 0)
                emptyText.text = "Сейчас никто не находится в вылазке.";
        }

        for (int i = 0; i < count; i++)
        {
            ExpeditionManager.ExpeditionRun run =
                expeditionManager.GetActiveExpeditionAt(i);

            if (run == null)
                continue;

            string characterName = run.character != null
                ? run.character.fullName
                : "Персонаж не найден";

            string locationName = run.location != null
                ? run.location.locationName
                : "Локация не найдена";

            ActiveExpeditionEntryUI entry =
                Instantiate(entryPrefab, entriesContent, false);

            entry.gameObject.SetActive(true);
            entry.SetData(
                characterName,
                locationName,
                run.daysRemaining);
        }
    }

    private void ClearEntries()
    {
        for (int i = entriesContent.childCount - 1; i >= 0; i--)
            Destroy(entriesContent.GetChild(i).gameObject);
    }
}