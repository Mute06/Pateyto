using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input System için gerekli kütüphane

[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
}

[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool triggerOnEnter = false;
    private bool playerIsNear = false;

    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }

    private void Update()
    {
        // YENÝ INPUT SÝSTEMÝ KONTROLÜ
        // Keyboard.current.eKey.wasPressedThisFrame -> "E" tuþuna basýldýðý aný yakalar
        if (!triggerOnEnter && playerIsNear && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!DialogueManager.Instance.isDialogueActive)
            {
                TriggerDialogue();
            }
            else
            {
                DialogueManager.Instance.DisplayNextDialogueLine();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNear = true;
            if (triggerOnEnter)
            {
                TriggerDialogue();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerIsNear = false;
        }
    }
}