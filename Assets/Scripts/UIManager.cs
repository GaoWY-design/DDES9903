using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Book Prompt")]
    public GameObject bookPromptRoot;
    public Text bookQuestionText;
    public Image yesButtonImage;
    public Text yesButtonText;

    [Header("Wallet Prompt")]
    public GameObject walletPromptRoot;
    public Text walletQuestionText;
    public Image noButtonImage;
    public Text noButtonText;

    [Header("Game Over")]
    public GameObject gameOverOverlay;
    public Text gameOverText;

    static readonly Color IdleGray = new Color(0.42f, 0.42f, 0.42f, 0.92f);
    static readonly Color PressedGreen = new Color(0.16f, 0.58f, 0.24f, 0.95f);

    void Awake()
    {
        Instance = this;
        AutoBindIfNeeded();
        HideBookPrompt();
        HideWalletPrompt();
        if (gameOverOverlay != null) gameOverOverlay.SetActive(false);
        ResetButtonColors();
    }

    void AutoBindIfNeeded()
    {
        if (bookPromptRoot == null)
        {
            var t = transform.Find("BookPrompt");
            if (t != null) bookPromptRoot = t.gameObject;
        }
        if (walletPromptRoot == null)
        {
            var t = transform.Find("WalletPrompt");
            if (t != null) walletPromptRoot = t.gameObject;
        }
        if (gameOverOverlay == null)
        {
            var t = transform.Find("GameOverOverlay");
            if (t != null) gameOverOverlay = t.gameObject;
        }

        if (bookQuestionText == null && bookPromptRoot != null)
            bookQuestionText = FindText(bookPromptRoot.transform, "QuestionText");
        if (yesButtonImage == null && bookPromptRoot != null)
        {
            var yes = bookPromptRoot.transform.Find("YesButton_KeyboardE");
            if (yes != null)
            {
                yesButtonImage = yes.GetComponent<Image>();
                yesButtonText = FindText(yes, "YesText");
            }
        }

        if (walletQuestionText == null && walletPromptRoot != null)
            walletQuestionText = FindText(walletPromptRoot.transform, "WalletQuestionText");
        if (noButtonImage == null && walletPromptRoot != null)
        {
            var no = walletPromptRoot.transform.Find("NoButton_KeyboardE");
            if (no != null)
            {
                noButtonImage = no.GetComponent<Image>();
                noButtonText = FindText(no, "NoText");
            }
        }

        if (gameOverText == null && gameOverOverlay != null)
            gameOverText = FindText(gameOverOverlay.transform, "GameOverText");
    }

    static Text FindText(Transform root, string name)
    {
        var t = root.Find(name);
        return t != null ? t.GetComponent<Text>() : null;
    }

    public void ShowBookPrompt()
    {
        if (bookPromptRoot != null) bookPromptRoot.SetActive(true);
        if (bookQuestionText != null) bookQuestionText.text = "Is this your book?";
        if (yesButtonText != null) yesButtonText.text = "Yes";
        if (yesButtonImage != null) yesButtonImage.color = IdleGray;
    }

    public void HideBookPrompt()
    {
        if (bookPromptRoot != null) bookPromptRoot.SetActive(false);
    }

    public void ShowWalletPrompt()
    {
        if (walletPromptRoot != null) walletPromptRoot.SetActive(true);
        if (walletQuestionText != null) walletQuestionText.text = "Is this your Wallet?";
        if (noButtonText != null) noButtonText.text = "No";
        if (noButtonImage != null) noButtonImage.color = IdleGray;
    }

    public void HideWalletPrompt()
    {
        if (walletPromptRoot != null) walletPromptRoot.SetActive(false);
    }

    public void FlashYesPressed(float seconds = 0.45f)
    {
        StartCoroutine(FlashButton(yesButtonImage, seconds));
    }

    public void FlashNoPressed(float seconds = 0.45f)
    {
        StartCoroutine(FlashButton(noButtonImage, seconds));
    }

    IEnumerator FlashButton(Image img, float seconds)
    {
        if (img != null) img.color = PressedGreen;
        yield return new WaitForSeconds(seconds);
        if (img != null) img.color = IdleGray;
    }

    public void ShowGameOver()
    {
        HideBookPrompt();
        HideWalletPrompt();
        if (gameOverOverlay != null) gameOverOverlay.SetActive(true);
        if (gameOverText != null) gameOverText.text = "GAME OVER!";
    }

    void ResetButtonColors()
    {
        if (yesButtonImage != null) yesButtonImage.color = IdleGray;
        if (noButtonImage != null) noButtonImage.color = IdleGray;
    }
}
