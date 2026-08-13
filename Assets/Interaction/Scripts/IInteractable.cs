using UnityEngine.InputSystem;

public interface IInteractable
{
    bool CanInteract(Interactor interactor);

    void Interact(Interactor interactor);

    string GetInteractionPrompt(Interactor interactor);

    InputActionReference GetInteractionAction();

    void OnHoverEnter(Interactor interactor);

    void OnHoverExit(Interactor interactor);
}