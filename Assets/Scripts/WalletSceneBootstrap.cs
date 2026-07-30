using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Runtime fallback: recreate wallet / GameUI if missing from the scene.
/// Does not override existing UI RectTransform layout when objects already exist.
/// </summary>
public class WalletSceneBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void EnsureCriticalObjects()
    {
        EnsureWallet();
        EnsureGameUI();
    }

    static void EnsureWallet()
    {
        if (GameObject.Find("BrownLeatherWallet") != null) return;

        var wallet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallet.name = "BrownLeatherWallet";
        wallet.transform.position = new Vector3(1.209f, 0.831f, 0.82f);
        wallet.transform.rotation = Quaternion.Euler(0f, 18f, 0f);
        wallet.transform.localScale = new Vector3(0.22f, 0.045f, 0.14f);
        wallet.AddComponent<WalletInteraction>();
        Debug.LogWarning("[WalletSceneBootstrap] BrownLeatherWallet was missing; created fallback.");
    }

    static void EnsureGameUI()
    {
        if (GameObject.Find("GameUI") != null) return;

        var canvasGo = new GameObject("GameUI");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<UIManager>();

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Debug.LogWarning("[WalletSceneBootstrap] GameUI was missing; created minimal fallback Canvas.");
    }
}
