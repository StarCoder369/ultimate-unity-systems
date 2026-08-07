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
    public TMP_Text currentObjTxt;

    [Header("Returned Fields | Should not change in Inspector")]
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

            if (currentObjTxt != null)
            {
                currentObjTxt.text = hit.transform.name;
            }
        }
        else
        {
            currentObject = null;
            currentHit = default;

            if (currentObjTxt != null)
            {
                currentObjTxt.text = "Null";
            }
        }
    }
}