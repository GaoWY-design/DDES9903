using UnityEngine;

public class BookInteraction : MonoBehaviour
{
    public static bool BookTaken { get; private set; }

    public float maxDistance = 2.15f;
    public float lookDotThreshold = 0.84f;
    public float rayDistance = 3.5f;
    public LayerMask rayMask = ~0;

    Transform _player;
    Camera _cam;
    UIManager _ui;
    bool _promptVisible;
    bool _busy;

    void Awake()
    {
        BookTaken = false;
    }

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
        if (BookTaken || _busy) return;
        if (_player == null || _cam == null || _ui == null)
        {
            ResolveRefs();
            return;
        }

        bool canInteract = CanSeeBook();
        if (canInteract && !_promptVisible)
        {
            _ui.ShowBookPrompt();
            _promptVisible = true;
        }
        else if (!canInteract && _promptVisible)
        {
            _ui.HideBookPrompt();
            _promptVisible = false;
        }

        if (canInteract && Input.GetKeyDown(KeyCode.E))
            TakeBook();
    }

    bool CanSeeBook()
    {
        Vector3 toBook = transform.position - _cam.transform.position;
        float dist = toBook.magnitude;
        if (dist > maxDistance) return false;

        Vector3 dir = toBook.normalized;
        if (Vector3.Dot(_cam.transform.forward, dir) < lookDotThreshold) return false;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, rayMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                return true;
        }

        // Very close fallback when ray barely misses due to thin mesh.
        return dist <= 1.15f && Vector3.Dot(_cam.transform.forward, dir) >= lookDotThreshold;
    }

    void TakeBook()
    {
        _busy = true;
        BookTaken = true;
        if (_ui != null)
        {
            _ui.FlashYesPressed(0.45f);
            _ui.HideBookPrompt();
        }
        _promptVisible = false;
        gameObject.SetActive(false);
    }

    public static void ResetBookTakenForEditorTests()
    {
        BookTaken = false;
    }

    public static void DebugSetBookTaken(bool taken)
    {
        BookTaken = taken;
    }
}
