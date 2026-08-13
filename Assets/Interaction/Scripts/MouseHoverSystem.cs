using UnityEngine;
using UnityEngine.InputSystem;

public enum InteractionDetectionMode
{
    Mouse,
    ScreenCenter
}

public class MouseHoverSystem : MonoBehaviour
{
    [Header("Detection")]
    public InteractionDetectionMode detectionMode = InteractionDetectionMode.Mouse;
    public Camera targetCamera;
    public LayerMask hoverLayers = ~0;
    public float maxDistance = 100f;

    [Header("Cursor")]
    public CustomCursor cursor;

    [HideInInspector]
    public GameObject currentObject;

    public RaycastHit currentHit;

    private InteractionController interactionController;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        interactionController = GetComponent<InteractionController>();
    }

    private void Update()
    {
        if (!TryGetDetectionRay(out Ray ray))
        {
            ClearHover();
            return;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hoverLayers))
        {
            currentObject = hit.collider.gameObject;
            currentHit = hit;

            UpdateDefaultCursor();
        }
        else
        {
            ClearHover();
        }
    }

    private bool TryGetDetectionRay(out Ray ray)
    {
        ray = default;

        if (targetCamera == null)
        {
            return false;
        }

        if (detectionMode == InteractionDetectionMode.Mouse)
        {
            if (Mouse.current == null)
            {
                return false;
            }

            ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            return true;
        }

        if (detectionMode == InteractionDetectionMode.ScreenCenter)
        {
            ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            return true;
        }

        return false;
    }

    private void ClearHover()
    {
        currentObject = null;
        currentHit = default;

        if (cursor != null && interactionController == null)
        {
            cursor.DisableText();
        }
    }

    private void UpdateDefaultCursor()
    {
        if (cursor == null)
        {
            return;
        }

        if (interactionController == null)
        {
            cursor.EnableText();
            cursor.SetText(currentObject.name);
        }
    }
}