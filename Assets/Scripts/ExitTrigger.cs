using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    bool _triggered;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!BookInteraction.BookTaken) return;

        var fps = other.GetComponent<FirstPersonController>();
        if (fps == null) fps = other.GetComponentInParent<FirstPersonController>();
        if (fps == null && !other.CompareTag("Player")) return;

        _triggered = true;
        if (fps != null) fps.SetControlsEnabled(false);

        var ui = UIManager.Instance != null ? UIManager.Instance : FindObjectOfType<UIManager>();
        if (ui != null) ui.ShowGameOver();
        else Debug.LogWarning("[ExitTrigger] UIManager missing; cannot show GAME OVER.");
    }
}
