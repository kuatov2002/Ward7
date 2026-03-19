using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    public static FirstPersonLook Instance { get; private set; }

    [SerializeField] float sensitivity = 2f;
    [SerializeField] float minYaw = -60f, maxYaw = 60f;
    [SerializeField] float minPitch = -20f, maxPitch = 30f;

    float _yaw, _pitch;
    bool _locked;

    void Awake()
    {
        Instance = this;
        var euler = transform.eulerAngles;
        _yaw = euler.y;
        _pitch = euler.x;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetLocked(bool locked)
    {
        _locked = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
    }

    void Update()
    {
        if (_locked) return;

        float mx = Input.GetAxis("Mouse X") * sensitivity;
        float my = Input.GetAxis("Mouse Y") * sensitivity;

        _yaw = Mathf.Clamp(_yaw + mx, minYaw, maxYaw);
        _pitch = Mathf.Clamp(_pitch - my, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
