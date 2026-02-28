using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.2f;

    public Animator animator;

    public GameObject nextButton; // "Sonraki" düðmesi
    private PlayerMovement playerController; // Oyuncunun kontrolünü saðlayan script

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        lines = new Queue<DialogueLine>();
    }

    private void Start()
    {
        nextButton.SetActive(false); // Düðmeyi baþlangýçta pasif yap
        playerController = FindObjectOfType<PlayerMovement>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;

        animator.Play("show");
        playerController.canMove = false; // Oyuncu hareketini kapat

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
        }
        DialogueLine currentLine = lines.Dequeue();
        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name;

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine));
    }

    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        nextButton.SetActive(false); // Düðmeyi pasif yap
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        nextButton.SetActive(true); // Tüm yazý tamamlandýktan sonra düðmeyi aktif yap
    }

    public void OnNextButtonPressed()
    {
        DisplayNextDialogueLine();
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        animator.Play("hide");
        playerController.canMove = true; // Oyuncu hareketini yeniden aktif et
    }
}
