using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door")]
    public bool isOpen;
    public float openAngle = 90f;
    public float rotationSpeed = 5f;
    public Vector3 rotationAxis = Vector3.up;
    public bool openAwayFromFront = true;

    [Header("Interaction")]
    public InputActionReference interactionAction;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isAnimating;

    private void Awake()
    {
        closedRotation = transform.localRotation;
        CalculateOpenRotation();
    }

    private void Update()
    {
        if (!isAnimating)
        {
            return;
        }

        transform.localRotation = Quaternion.Lerp(transform.localRotation, openRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.localRotation, openRotation) < 0.1f)
        {
            transform.localRotation = openRotation;
            isAnimating = false;
        }
    }

    public bool CanInteract(Interactor interactor)
    {
        return !isAnimating;
    }

    public void Interact(Interactor interactor)
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            CalculateOpenRotation();
        }

        isAnimating = true;
    }

    public string GetInteractionPrompt(Interactor interactor)
    {
        if (isAnimating)
        {
            return "Please be patient for the door to open fully. Give it time...";
        }

        if (isOpen)
        {
            return "Close Door";
        }

        return "Open Door";
    }

    public InputActionReference GetInteractionAction()
    {
        return interactionAction;
    }

    public void OnHoverEnter(Interactor interactor)
    {
        Debug.Log("Started hovering over door.");

        // Start effects like outline, highlight, etc.
    }

    public void OnHoverExit(Interactor interactor)
    {
        Debug.Log("Stopped hovering over door.");

        // Stop effects like outline, highlight, etc.
    }

    private void CalculateOpenRotation()
    {
        float direction = openAwayFromFront ? 1f : -1f;
        Vector3 rotation = rotationAxis.normalized * openAngle * direction;
        openRotation = closedRotation * Quaternion.Euler(rotation);
    }
}