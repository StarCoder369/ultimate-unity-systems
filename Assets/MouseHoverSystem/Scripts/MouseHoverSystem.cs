using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHoverSystem : MonoBehaviour
{
    [Header("Customizable Fields")]
    public Camera targetCamera;
    public LayerMask hoverLayers = ~0;
    public float maxDistance = 100f;
    [Tooltip("Optional")]
    public CustomCursor cursor;

    //other stuff
    [HideInInspector]
    public GameObject currentObject;
    public RaycastHit currentHit;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hoverLayers))
        {
            currentObject = hit.collider.gameObject;
            currentHit = hit;

            if (cursor != null)
            {
                cursor.EnableText();
                cursor.SetText(hit.transform.name);
            }
        }
        else
        {
            currentObject = null;
            currentHit = default;

            if (cursor != null)
            {
                cursor.DisableText();
            }
        }
    }
}