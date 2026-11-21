using UnityEngine;

public class ExpeditionPagesSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject expeditionsPage;
    [SerializeField] private GameObject activeExpeditionsPage;

    public void ShowExpeditionsPage()
    {
        if (expeditionsPage != null)
            expeditionsPage.SetActive(true);

        if (activeExpeditionsPage != null)
            activeExpeditionsPage.SetActive(false);
    }

    public void ShowActiveExpeditionsPage()
    {
        if (expeditionsPage != null)
            expeditionsPage.SetActive(false);

        if (activeExpeditionsPage != null)
            activeExpeditionsPage.SetActive(true);
    }
}