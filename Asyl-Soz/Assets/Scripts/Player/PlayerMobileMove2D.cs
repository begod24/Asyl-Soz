using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMobileMove2D : MonoBehaviour
{
    public enum InputMode { Drag, Tilt }

    [Header("Mode")]
    [UnityEngine.SerializeField] private InputMode mode = InputMode.Drag;

    [Header("Input Actions")]
    [UnityEngine.SerializeField] private InputActionReference pointAction; // Pointer/position
    [UnityEngine.SerializeField] private InputActionReference pressAction; // Pointer/press
    [UnityEngine.SerializeField] private InputActionReference tiltAction;  // Accelerometer/acceleration

    [Header("Movement")]
    [UnityEngine.SerializeField] private float moveSpeed = 6f;

    [Header("Drag")]
    [UnityEngine.SerializeField] private float dragSensitivity = 0.35f;

    [Header("Tilt")]
    [UnityEngine.SerializeField] private float tiltSensitivity = 1.7f;

    [Header("Smoothing")]
    [UnityEngine.SerializeField] private float inputSmoothing = 12f;

    private Rigidbody2D rb;
    private Vector2 dragStartPos;
    private float currentInputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        pointAction?.action.Enable();
        pressAction?.action.Enable();
        tiltAction?.action.Enable();

        if (Accelerometer.current != null)
            InputSystem.EnableDevice(Accelerometer.current);
    }

    private void OnDisable()
    {
        pointAction?.action.Disable();
        pressAction?.action.Disable();
        tiltAction?.action.Disable();
    }

    private void Update()
    {
        float targetX = (mode == InputMode.Drag) ? ReadDragX() : ReadTiltX();

        float t = 1f - Mathf.Exp(-inputSmoothing * Time.deltaTime);
        currentInputX = Mathf.Lerp(currentInputX, targetX, t);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(currentInputX * moveSpeed, rb.linearVelocity.y);
    }

    private float ReadDragX()
    {
        if (pointAction == null || pressAction == null) return 0f;

        bool isPressed = pressAction.action.ReadValue<float>() > 0.5f;

        if (!isPressed)
        {
            dragStartPos = Vector2.zero; // IMPORTANT: reset on release
            return 0f;
        }

        Vector2 pointerPos = pointAction.action.ReadValue<Vector2>();

        if (dragStartPos == Vector2.zero)
            dragStartPos = pointerPos;

        float deltaX = pointerPos.x - dragStartPos.x;
        float norm = deltaX / (Screen.width * dragSensitivity);

        return Mathf.Clamp(norm, -1f, 1f);
    }

    private float ReadTiltX()
    {
        if (tiltAction == null) return 0f;

        Vector3 accel = tiltAction.action.ReadValue<Vector3>();
        float x = accel.x * tiltSensitivity;

        return Mathf.Clamp(x, -1f, 1f);
    }

    public void SetModeDrag()
    {
        mode = InputMode.Drag;
        dragStartPos = Vector2.zero;
        currentInputX = 0f;
    }

    public void SetModeTilt()
    {
        mode = InputMode.Tilt;
        dragStartPos = Vector2.zero;
        currentInputX = 0f;
    }
}