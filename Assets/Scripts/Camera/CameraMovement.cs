using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineConfiner2D confiner;


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float fastMoveSpeed = 40f;
    [SerializeField] private float edgeScrollSpeed = 15f;
    [SerializeField] private float dragSpeed = 1f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Edge Scrolling")]
    [SerializeField] private bool enableEdgeScrolling = true;
    [SerializeField] private float edgeThreshold = 20f;

    private InputSystem_Actions inputActions;
    private bool isDragging;
    private Vector3 lastMousePosition;
    private Camera cam;
    [SerializeField] private Transform cameraTransform;
    private Collider2D boundingShape;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        cam = Camera.main;

        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (virtualCamera != null && confiner == null)
        {
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
        }

        if (confiner != null)
        {
            boundingShape = confiner.BoundingShape2D;
        }

        // Use the parent transform as the camera rig if available
        cameraTransform = transform.parent != null ? transform.parent : transform;

        // Set the virtual camera to follow the parent (moving object)
        if (virtualCamera != null)
        {
            virtualCamera.Follow = cameraTransform;
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        // Set up Cinemachine virtual camera for 2D top-down

        Cursor.lockState = CursorLockMode.Confined;
        
    }

    void Update()
    {
         
        HandleKeyboardMovement();
        HandleEdgeScrolling();
        HandleMouseDrag();
        HandleZoom();
    }


    private void HandleKeyboardMovement()
    {
        // WASD movement
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            vertical = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            vertical = -1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontal = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontal = 1f;

        Vector3 moveDirection = new Vector3(horizontal, vertical, 0f).normalized;

        // Sprint with Shift
        float currentSpeed = Keyboard.current.leftShiftKey.isPressed ? fastMoveSpeed : moveSpeed;

        // Move camera rig in 2D space
        Vector3 newPosition = cameraTransform.position + moveDirection * currentSpeed * Time.unscaledDeltaTime;
        cameraTransform.position = ClampPositionToBounds(newPosition);
    }

    private void HandleEdgeScrolling()
    {
        if (!enableEdgeScrolling)
            return;

        Vector3 edgeMove = Vector3.zero;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Check screen edges
        if (mousePosition.x < edgeThreshold)
            edgeMove.x = -1f;
        else if (mousePosition.x > Screen.width - edgeThreshold)
            edgeMove.x = 1f;

        if (mousePosition.y < edgeThreshold)
            edgeMove.y = -1f;
        else if (mousePosition.y > Screen.height - edgeThreshold)
            edgeMove.y = 1f;

        if (edgeMove != Vector3.zero)
        {
            Vector3 newPosition = cameraTransform.position + edgeMove * edgeScrollSpeed * Time.unscaledDeltaTime;
            cameraTransform.position = ClampPositionToBounds(newPosition);
        }
    }

    private void HandleMouseDrag()
    {
        if (cam == null)
            return;

        // Middle mouse button or right mouse button drag
        if (Mouse.current.middleButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        if ((Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed) && isDragging)
        {
            Vector3 currentMousePos = Mouse.current.position.ReadValue();
            Vector3 mouseDelta = currentMousePos - lastMousePosition;

            // Convert screen space delta to world space movement
            float worldDeltaX = -mouseDelta.x * dragSpeed * virtualCamera.Lens.OrthographicSize / Screen.height;
            float worldDeltaY = -mouseDelta.y * dragSpeed * virtualCamera.Lens.OrthographicSize / Screen.height;

            Vector3 newPosition = cameraTransform.position + new Vector3(worldDeltaX, worldDeltaY, 0f);
            cameraTransform.position = ClampPositionToBounds(newPosition);

            lastMousePosition = currentMousePos;
        }

        if (Mouse.current.middleButton.wasReleasedThisFrame || Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void HandleZoom()
    {
        if (virtualCamera == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            float newSize = virtualCamera.Lens.OrthographicSize - scroll * zoomSpeed * Time.unscaledDeltaTime;
            virtualCamera.Lens.OrthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private Vector3 ClampPositionToBounds(Vector3 position)
    {
        // If no bounding shape is set, return the position unchanged
        if (boundingShape == null)
            return position;

        // Get the closest point on the collider to the desired position
        Vector2 clampedPosition2D = boundingShape.ClosestPoint(position);

        // Return the clamped position with the original Z coordinate
        return new Vector3(clampedPosition2D.x, clampedPosition2D.y, position.z);
    }
}