using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 1.9f;
    public float initialPitch = 3f;

    [Header("Move")]
    public float moveSpeed = 2.1f;
    public float gravity = -18f;

    [Header("Safety")]
    public float fallResetY = -0.35f;

    CharacterController _cc;
    float _yaw;
    float _pitch;
    float _verticalVelocity;
    Vector3 _lastSafePosition;
    bool _controlsEnabled = true;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null) playerCamera = cam.transform;
        }

        _yaw = transform.eulerAngles.y;
        _pitch = initialPitch;
        _lastSafePosition = transform.position;
        ApplyLook();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!_controlsEnabled) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
            _yaw += mx;
            _pitch -= my;
            _pitch = Mathf.Clamp(_pitch, -85f, 85f);
            ApplyLook();
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * h + transform.forward * v).normalized * moveSpeed;

        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
        _verticalVelocity += gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * Time.deltaTime);

        if (transform.position.y >= fallResetY)
            _lastSafePosition = transform.position;
        else
        {
            _cc.enabled = false;
            transform.position = _lastSafePosition;
            _cc.enabled = true;
            _verticalVelocity = 0f;
        }
    }

    void ApplyLook()
    {
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public void SetControlsEnabled(bool enabled)
    {
        _controlsEnabled = enabled;
        if (!enabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public bool ControlsEnabled => _controlsEnabled;
}
