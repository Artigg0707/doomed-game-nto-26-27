using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Контроллер главного меню. Повесьте на любой GameObject в сцене MainMenu.
/// Кнопки настраиваются через инспектор (OnClick -> MenuManager).
/// </summary>
public class MenuManager : MonoBehaviour
{
    private static MenuManager _instance;
    public static MenuManager Instance => _instance;

    [Header("Сцена игры")]
    [Tooltip("Имя сцены с игровым процессом (как в Build Settings)")]
    [SerializeField]
    private string gameSceneName = "GameScene 1";

    [Header("Сохранение")]
    [SerializeField]
    private string saveSlot = "save1";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>Новая игра: сбросить текущий слот и загрузить сцену игры.</summary>
    public void NewGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.ResetCurrentSlot();
        }
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>Загрузить последнее сохранение и перейти в игру.</summary>
    public void LoadLastSave()
    {
        if (SaveSystem.Instance == null)
        {
            Debug.LogWarning("[MenuManager] SaveSystem не найден на сцене");
            return;
        }

        if (!SaveSystem.Instance.HasSave(saveSlot))
        {
            Debug.LogWarning($"[MenuManager] Сохранение слота '{saveSlot}' не найдено");
            return;
        }

        SaveSystem.Instance.LoadCurrentSlot();
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>Открыть настройки (заглушка — подключите свою панель настроек).</summary>
    public void OpenSettings()
    {
        Debug.Log("[MenuManager] Открытие настроек — пока заглушка");
    }

    /// <summary>Выйти из игры.</summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
