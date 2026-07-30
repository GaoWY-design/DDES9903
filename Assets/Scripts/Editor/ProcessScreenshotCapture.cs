#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Captures 6 process screenshots onto the Desktop.
/// Menu: Tools / Lost And Found / Capture Process Screenshots
/// </summary>
public static class ProcessScreenshotCapture
{
    static string ShotDir
    {
        get
        {
            // Prefer ASCII path to avoid console/codepage issues; also mirror to Chinese folder.
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
                "LostAndFound_ProcessShots");
        }
    }

    static string ShotDirChinese
    {
        get
        {
            return Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory),
                "失物招领_过程截图");
        }
    }

    const string ScenePath = "Assets/Scenes/LostAndFound.unity";

    [MenuItem("Tools/Lost And Found/Capture Process Screenshots")]
    public static void CaptureFromMenu()
    {
        CaptureBatchInternal(exitWhenDone: false);
        EditorUtility.DisplayDialog("失物招领", "过程截图已保存到桌面:\n" + ShotDir + "\n与\n" + ShotDirChinese, "OK");
    }

    /// <summary>Batchmode entry: -executeMethod ProcessScreenshotCapture.CaptureBatch</summary>
    public static void CaptureBatch()
    {
        CaptureBatchInternal(exitWhenDone: true);
    }

    static void CaptureBatchInternal(bool exitWhenDone)
    {
        EnsureDir();
        LostAndFoundSceneBuilder.BuildScene();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var cam = Object.FindObjectOfType<Camera>();
        if (cam == null)
        {
            Debug.LogError("[ProcessScreenshot] No camera in scene.");
            if (exitWhenDone) EditorApplication.Exit(1);
            return;
        }

        // Keep a clean helper camera pose bookkeeping
        var player = GameObject.Find("Player_FirstPerson");
        var book = GameObject.Find("BlueHardcoverBook_ShiyuJi");
        var ui = Object.FindObjectOfType<UIManager>();

        // 1) Room overview from back-right corner
        PoseCam(cam, new Vector3(1.85f, 1.55f, -4.2f), new Vector3(8f, -20f, 0f));
        HidePrompts(ui);
        RenderCam(cam, "01_工程地基_场景打开.png");

        // 2) Looking at front wall / door geometry from center
        PoseCam(cam, new Vector3(0f, 1.45f, -1.4f), new Vector3(5f, 0f, 0f));
        RenderCam(cam, "02_房间几何_四面墙与后门洞.png");

        // 3) Table / book / wallet / warm light
        PoseCam(cam, new Vector3(1.35f, 1.35f, -0.55f), Quaternion.LookRotation(
            new Vector3(0f, 0.84f, 0.92f) - new Vector3(1.35f, 1.35f, -0.55f)));
        RenderCam(cam, "03_桌子道具灯光氛围.png");

        // 4) Player start framing (restore player camera local)
        if (player != null)
        {
            cam.transform.SetParent(player.transform, false);
            cam.transform.localPosition = new Vector3(0f, 1.58f, 0f);
            cam.transform.localRotation = Quaternion.Euler(3f, 0f, 0f);
            player.transform.position = new Vector3(0f, 0.035f, -2.75f);
            player.transform.rotation = Quaternion.identity;
        }
        else
        {
            PoseCam(cam, new Vector3(0f, 1.615f, -2.75f), new Vector3(3f, 0f, 0f));
        }
        RenderCam(cam, "04_玩家开局构图_面向桌与墙字.png");

        // 5) Book prompt near table (UI must be Screen Space - Camera to appear in Render)
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.035f, -0.35f);
            cam.transform.localPosition = new Vector3(0f, 1.58f, 0f);
            if (book != null)
                cam.transform.rotation = Quaternion.LookRotation(book.transform.position - cam.transform.position);
        }
        else if (book != null)
        {
            PoseCam(cam, new Vector3(0f, 1.55f, -0.2f), Quaternion.LookRotation(book.transform.position - new Vector3(0f, 1.55f, -0.2f)));
        }
        var canvas = ui != null ? ui.GetComponent<Canvas>() : Object.FindObjectOfType<Canvas>();
        var prevMode = RenderMode.ScreenSpaceOverlay;
        Camera prevWorldCam = null;
        if (canvas != null)
        {
            prevMode = canvas.renderMode;
            prevWorldCam = canvas.worldCamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 0.35f;
        }
        if (ui != null) ui.ShowBookPrompt();
        Canvas.ForceUpdateCanvases();
        RenderCam(cam, "05_书本交互提示_IsThisYourBook.png");
        if (ui != null) ui.HideBookPrompt();

        // 6) Exit / Game Over
        if (player != null)
        {
            player.transform.position = new Vector3(0f, 0.035f, -5.15f);
            player.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            cam.transform.localPosition = new Vector3(0f, 1.58f, 0f);
            cam.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);
        }
        else
        {
            PoseCam(cam, new Vector3(0f, 1.615f, -5.15f), new Vector3(5f, 180f, 0f));
        }
        if (ui != null) ui.ShowGameOver();
        Canvas.ForceUpdateCanvases();
        RenderCam(cam, "06_出口GameOver_结束画面.png");

        if (canvas != null)
        {
            canvas.renderMode = prevMode;
            canvas.worldCamera = prevWorldCam;
        }

        WriteIndex();
        Debug.Log("[ProcessScreenshot] Saved screenshots to: " + ShotDir + " and " + ShotDirChinese);

        // Restore scene file without temporary pose edits
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        if (exitWhenDone) EditorApplication.Exit(0);
    }

    static void EnsureDir()
    {
        Directory.CreateDirectory(ShotDir);
        Directory.CreateDirectory(ShotDirChinese);
    }

    static void HidePrompts(UIManager ui)
    {
        if (ui == null) return;
        ui.HideBookPrompt();
        ui.HideWalletPrompt();
        if (ui.gameOverOverlay != null) ui.gameOverOverlay.SetActive(false);
    }

    static void PoseCam(Camera cam, Vector3 pos, Vector3 euler)
    {
        cam.transform.SetParent(null, true);
        cam.transform.position = pos;
        cam.transform.rotation = Quaternion.Euler(euler);
    }

    static void PoseCam(Camera cam, Vector3 pos, Quaternion rot)
    {
        cam.transform.SetParent(null, true);
        cam.transform.position = pos;
        cam.transform.rotation = rot;
    }

    static void RenderCam(Camera cam, string fileName)
    {
        int w = 1600;
        int h = 900;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        var prev = cam.targetTexture;
        bool prevEnabled = cam.enabled;
        cam.enabled = true;
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        cam.targetTexture = prev;
        cam.enabled = prevEnabled;
        RenderTexture.active = null;
        Object.DestroyImmediate(rt);

        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string p1 = Path.Combine(ShotDir, fileName);
        string p2 = Path.Combine(ShotDirChinese, fileName);
        File.WriteAllBytes(p1, png);
        File.WriteAllBytes(p2, png);
        Debug.Log("[ProcessScreenshot] Saved: " + p1);
    }

    static void WriteIndex()
    {
        string readme =
            "失物招领 v0.1 — 过程截图\n" +
            "1. 01_工程地基_场景打开.png\n" +
            "2. 02_房间几何_四面墙与后门洞.png\n" +
            "3. 03_桌子道具灯光氛围.png\n" +
            "4. 04_玩家开局构图_面向桌与墙字.png\n" +
            "5. 05_书本交互提示_IsThisYourBook.png\n" +
            "6. 06_出口GameOver_结束画面.png\n" +
            "\nUnity 2022.3.62f3c1 | Built-in | Tools/Lost And Found/Build Scene\n" +
            "英文目录副本: Desktop/LostAndFound_ProcessShots\n";
        File.WriteAllText(Path.Combine(ShotDir, "截图说明.txt"), readme, System.Text.Encoding.UTF8);
        File.WriteAllText(Path.Combine(ShotDirChinese, "截图说明.txt"), readme, System.Text.Encoding.UTF8);
    }
}
#endif
