using System.Collections.Generic;
using UnityEngine;

public class PCMenu : MonoBehaviour, IOpenablePanel
{
    [SerializeField] private List<MonoBehaviour> tabComponents = new List<MonoBehaviour>();
    private readonly List<IPCTab> _tabs = new List<IPCTab>();

    private void Awake()
    {
        foreach (var mb in tabComponents)
            if (mb is IPCTab t) _tabs.Add(t);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        foreach (var t in _tabs) t.Refresh();
        if (_tabs.Count > 0) _tabs[0].Show();
    }

    public void Close() { gameObject.SetActive(false); }

    public void SelectTab(int index)
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            if (i == index) _tabs[i].Show(); else _tabs[i].Hide();
        }
    }
}