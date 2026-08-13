using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour
{
    [Header("References")]
    public MouseHoverSystem mouseHoverSystem;
    public Interactor interactor;
    public CustomCursor cursor;

    [Header("Interaction")]
    public InputActionReference defaultInteractionAction;

    private IInteractable currentInteractable;
    private IInteractable previousInteractable;

    private void Awake()
    {
        if (mouseHoverSystem == null)
        {
            mouseHoverSystem = GetComponent<MouseHoverSystem>();
        }

        if (interactor == null)
        {
            interactor = GetComponent<Interactor>();
        }
    }

    private void Update()
    {
        FindInteractable();
        HandleHover();
        UpdateCursor();
        HandleInteraction();
    }

    private void FindInteractable()
    {
        previousInteractable = currentInteractable;
        currentInteractable = null;

        if (mouseHoverSystem == null)
        {
            return;
        }

        if (mouseHoverSystem.currentObject == null)
        {
            return;
        }

        currentInteractable = mouseHoverSystem.currentObject.GetComponentInParent<IInteractable>();
    }

    private void HandleHover()
    {
        if (previousInteractable == currentInteractable)
        {
            return;
        }

        if (previousInteractable != null)
        {
            previousInteractable.OnHoverExit(interactor);
        }

        if (currentInteractable != null)
        {
            currentInteractable.OnHoverEnter(interactor);
        }
    }

    private void UpdateCursor()
    {
        if (cursor == null)
        {
            return;
        }

        if (mouseHoverSystem == null || mouseHoverSystem.currentObject == null)
        {
            cursor.DisableText();
            return;
        }

        cursor.EnableText();

        if (currentInteractable != null)
        {
            cursor.SetText(currentInteractable.GetInteractionPrompt(interactor));
        }
        else
        {
            cursor.SetText(mouseHoverSystem.currentObject.name);
        }
    }

    private void HandleInteraction()
    {
        if (currentInteractable == null)
        {
            return;
        }

        if (!currentInteractable.CanInteract(interactor))
        {
            return;
        }

        InputActionReference interactionAction = GetInteractionAction();

        if (interactionAction == null)
        {
            return;
        }

        if (interactionAction.action.WasPressedThisFrame())
        {
            currentInteractable.Interact(interactor);
        }
    }

    private InputActionReference GetInteractionAction()
    {
        if (currentInteractable == null)
        {
            return defaultInteractionAction;
        }

        InputActionReference interactionAction = currentInteractable.GetInteractionAction();

        if (interactionAction == null)
        {
            return defaultInteractionAction;
        }

        return interactionAction;
    }
}