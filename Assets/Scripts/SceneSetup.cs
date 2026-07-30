using UnityEngine;

public class SceneSetup : MonoBehaviour
{
    [TextArea(6, 20)]
    public string notes =
        "失物招领 / Lost And Found v0.1\n" +
        "开局面向文字墙与桌子；身后后门为出口。\n" +
        "E 认领书本后离开后门触发 GAME OVER。\n" +
        "UI 布局可在 Scene/Hierarchy 手调；运行时只改显隐与按钮颜色。\n" +
        "菜单：Tools / Lost And Found / Build Scene";

    public FirstPersonController player;
    public BookInteraction book;
    public WalletInteraction wallet;
    public ExitTrigger exitTrigger;
    public UIManager uiManager;
    public AudioSource musicSource;

    void Reset()
    {
        AutoFind();
    }

    void Awake()
    {
        AutoFind();
    }

    void AutoFind()
    {
        if (player == null) player = FindObjectOfType<FirstPersonController>();
        if (book == null) book = FindObjectOfType<BookInteraction>();
        if (wallet == null) wallet = FindObjectOfType<WalletInteraction>();
        if (exitTrigger == null) exitTrigger = FindObjectOfType<ExitTrigger>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (musicSource == null)
        {
            var go = GameObject.Find("GameAudio_Music3");
            if (go != null) musicSource = go.GetComponent<AudioSource>();
        }
    }
}
