using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatScreenPanel : MonoBehaviour
{
    public static DefeatScreenPanel Instance { get; private set; }

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
        Instance = this;

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

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        if (titleText != null)
            titleText.text = "ПОРАЖЕНИЕ";

        if (subtitleText != null)
            subtitleText.text = "Все жители погибли";

        gameObject.SetActive(true);
    }

    public void ShowVictory()
    {
        if (titleText != null)
            titleText.text = "ПОБЕДА";

        if (subtitleText != null)
            subtitleText.text = "Вы дождались гуманитарного конвоя.";

        gameObject.SetActive(true);
    }

    public void ShowCustomDefeatReason(string reason, string subtitle = null)
    {
        if (titleText != null)
            titleText.text = reason;

        if (subtitleText != null)
            subtitleText.text = subtitle != null ? subtitle : reason;

        gameObject.SetActive(true);
    }

    public void ShowDefeat()
    {
        ShowCustomDefeatReason("Все жители погибли");
    }

    public void ShowVictoryWithStory(int daysSurvived, int survivorsCount, int deathsCount, int exiledCount)
    {
        if (titleText != null)
            titleText.text = "ПОБЕДА";

        string story;
        if (survivorsCount > 0)
        {
            story = $"Прожито дней: {daysSurvived}\n" +
                    $"Выжило людей: {survivorsCount}\n" +
                    $"Погибло: {deathsCount}\n" +
                    $"Ушло из бункера: {exiledCount}\n\n" +
                    $"Вы дождались спасения. Гуманитарный конвой доставил еду, воду и лекарства.\n" +
                    $"Бункер выжил. Вы прошли путь от отчаяния к надежде.";
        }
        else
        {
            story = $"Прожито дней: {daysSurvived}\n" +
                    $"Выжило людей: 0\n" +
                    $"Погибло: {deathsCount}\n" +
                    $"Ушло из бункера: {exiledCount}\n\n" +
                    $"Конвой прибыл слишком поздно. Бункер пуст.";
        }

        if (subtitleText != null)
            subtitleText.text = story;

        gameObject.SetActive(true);
    }

    public void ShowDefeatWithStory(int daysSurvived, int survivorsCount, int deathsCount, int exiledCount, string defeatReason)
    {
        if (titleText != null)
            titleText.text = defeatReason;

        string story = $"Прожито дней: {daysSurvived}\n" +
                       $"Выжило людей: {survivorsCount}\n" +
                       $"Погибло: {deathsCount}\n" +
                       $"Ушло из бункера: {exiledCount}\n\n";

        string ending;
        if (deathsCount > 0 && exiledCount > 0)
        {
            ending = "Бункер пал. Некоторые жители пали в бою за выживание, другие не выдержали и ушли в пустошь.\n" +
                     "Надежда исчезла вместе с последними огнями.";
        }
        else if (deathsCount > 0)
        {
            ending = "Все жители погибли. Голод, холод и болезни одержали верх над последней группой выживших.\n" +
                     "Бункер замолчал навсегда.";
        }
        else if (exiledCount > 0)
        {
            ending = "Все жители ушли из бункера. Мораль полностью иссякла, и никто больше не верил в спасение.\n" +
                     "Пустошь поглотила всех, кто曾经 называл этот дом своим.";
        }
        else
        {
            ending = "Игра окончена. В бункере больше не осталось никого.";
        }

        story += ending;

        if (subtitleText != null)
            subtitleText.text = story;

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
