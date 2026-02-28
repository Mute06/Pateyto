using UnityEngine;

public interface IInteractable
{
    // Called when the player presses the interact key
    void Interact(PlayerInteractor interactor);
    
    // Optional: Called when the player gets within range to show a prompt (like "Press E")
    void ToggleInteractPrompt(bool show);
}
