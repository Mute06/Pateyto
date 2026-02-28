using UnityEngine;

public class JigsawInteractable : MonoBehaviour, IInteractable
{
    [Header("Puzzle Setup")]
    public GameObject jigsawUI_Prefab;
    public Texture2D specificPuzzleTexture;
    
    [Header("UI Prompts")]
    public GameObject interactPromptUI; 

    private JigsawManager activePuzzleManager;
    
    public void Interact(PlayerInteractor interactor)
    {
        if (activePuzzleManager != null) return; // Prevent spawning multiple puzzles

        // 1. Disable player from interacting twice or moving
        interactor.canInteract = false;
        
        // TODO: Disable top-down movement if you have a top-down movement script.
        // e.g. interactor.GetComponent<TopDownMovementScript>().enabled = false;

        // 2. Spawn the puzzle UI
        GameObject jigsawInstance = Instantiate(jigsawUI_Prefab);
        activePuzzleManager = jigsawInstance.GetComponentInChildren<JigsawManager>();

        // 3. Setup event listeners so we know when the player finishes or exits
        activePuzzleManager.OnPuzzleExit.AddListener(() => 
        {
            // Restore player controls when puzzle is quit
            interactor.canInteract = true;
            activePuzzleManager = null;
            // TODO: Re-enable player movement
        });

        activePuzzleManager.OnPuzzleComplete.AddListener(() => 
        {
            Debug.Log("World Object knows the puzzle was solved!");
            interactor.canInteract = true; 
            activePuzzleManager = null;
            
            // You can also invoke another UnityEvent here to unlock doors, play sound, etc.
        });

        // 4. Start the game
        activePuzzleManager.StartGame(specificPuzzleTexture);
        ToggleInteractPrompt(false);
    }

    public void ToggleInteractPrompt(bool show)
    {
        // Shows a world-space UI or Canvas UI element that says "Press E"
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(show);
        }
    }
}
