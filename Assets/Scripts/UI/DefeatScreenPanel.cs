using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatScreenPanel : MonoBehaviour
{
    [Header("Анимация Aseprite")]
    [SerializeField] private Image animationImage;
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField, Min(0f)] private float framesPerSecond = 12f;
    [SerializeField] private bool loopAnimation = true;

    [Header("Текст")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Кнопки")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    private float frameTimer;
    private int frameIndex;

    private void Awake()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        if (titleText != null)
            titleText.text = "ПОРАЖЕНИЕ";

        if (subtitleText != null)
            subtitleText.text = "Все жители погибли";

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        frameIndex = 0;
        frameTimer = 0f;
        ApplyFrame();
    }

    private void Update()
    {
        if (animationFrames == null ||
            animationFrames.Length == 0 ||
            framesPerSecond <= 0f)
            return;

        if (!loopAnimation &&
            frameIndex >= animationFrames.Length - 1)
            return;

        frameTimer += Time.unscaledDeltaTime;
        float frameDuration = 1f / framesPerSecond;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex++;

            if (frameIndex >= animationFrames.Length)
            {
                frameIndex = loopAnimation
                    ? 0
                    : animationFrames.Length - 1;
            }

            ApplyFrame();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ApplyFrame()
    {
        if (animationImage == null ||
            animationFrames == null ||
            animationFrames.Length == 0)
            return;

        frameIndex = Mathf.Clamp(
            frameIndex,
            0,
            animationFrames.Length - 1);
        animationImage.sprite = animationFrames[frameIndex];
        animationImage.enabled = animationImage.sprite != null;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
