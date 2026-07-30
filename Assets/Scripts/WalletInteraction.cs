using UnityEngine;

public class WalletInteraction : MonoBehaviour
{
    public bool WalletChecked { get; private set; }

    public float maxDistance = 1.9f;
    public float lookDotThreshold = 0.84f;
    public float rayDistance = 3.2f;
    public LayerMask rayMask = ~0;

    Transform _player;
    Camera _cam;
    UIManager _ui;
    bool _promptVisible;
    bool _lookingAwayRequired;
    bool _busy;

    void Start()
    {
        ResolveRefs();
    }

    void ResolveRefs()
    {
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) _player = playerGo.transform;
        if (_cam == null)
        {
            var fps = FindObjectOfType<FirstPersonController>();
            if (fps != null && fps.playerCamera != null)
                _cam = fps.playerCamera.GetComponent<Camera>();
            if (_cam == null) _cam = Camera.main;
        }
        if (_ui == null) _ui = UIManager.Instance != null ? UIManager.Instance : FindObjectOfType<UIManager>();
    }

    void Update()
    {
        if (_busy) return;
        if (_player == null || _cam == null || _ui == null)
        {
            ResolveRefs();
            return;
        }

        bool looking = IsLookingAtWallet();

        if (_lookingAwayRequired)
        {
            if (!looking)
                _lookingAwayRequired = false;
            if (_promptVisible)
            {
                _ui.HideWalletPrompt();
                _promptVisible = false;
            }
            return;
        }

        if (looking && !_promptVisible)
        {
            _ui.ShowWalletPrompt();
            _promptVisible = true;
        }
        else if (!looking && _promptVisible)
        {
            _ui.HideWalletPrompt();
            _promptVisible = false;
        }

        if (looking && Input.GetKeyDown(KeyCode.E))
            DenyWallet();
    }

    bool IsLookingAtWallet()
    {
        Vector3 toWallet = transform.position - _cam.transform.position;
        float dist = toWallet.magnitude;
        if (dist > maxDistance) return false;

        Vector3 dir = toWallet.normalized;
        if (Vector3.Dot(_cam.transform.forward, dir) < lookDotThreshold) return false;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, rayMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return true;
        }

        return dist <= 1.05f && Vector3.Dot(_cam.transform.forward, dir) >= lookDotThreshold;
    }

    void DenyWallet()
    {
        _busy = true;
        WalletChecked = true;
        if (_ui != null)
        {
            _ui.FlashNoPressed(0.45f);
            _ui.HideWalletPrompt();
        }
        _promptVisible = false;
        _lookingAwayRequired = true;
        // Wallet stays in scene.
        Invoke(nameof(ClearBusy), 0.45f);
    }

    void ClearBusy()
    {
        _busy = false;
    }
}
