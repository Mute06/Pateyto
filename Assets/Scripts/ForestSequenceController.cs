using UnityEngine;
using System.Collections;
using System.Reflection;

public class ForestSequenceController : MonoBehaviour
{
    [Header("Sequence Points")]
    public Transform square1;
    public Transform square2;
    public Collider2D square3Trigger;

    [Header("Settings")]
    public float autoWalkSpeed = 3f;
    public string playerTag = "Player";

    private bool sequenceStarted = false;
    private GameObject playerObj;
    private Animator playerAnim;
    private Rigidbody2D playerRb;
    private MonoBehaviour movementScript;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!sequenceStarted && other.CompareTag(playerTag))
        {
            sequenceStarted = true;
            InitializeAndStart(other.gameObject);
        }
    }

    private void InitializeAndStart(GameObject playerInstance)
    {
        playerObj = playerInstance;
        playerAnim = playerObj.GetComponentInChildren<Animator>();
        playerRb = playerObj.GetComponent<Rigidbody2D>();

        movementScript =(MonoBehaviour)playerObj.GetComponent("PlayerMovement");

        StartCoroutine(SequenceRoutine());
    }

    private RigidbodyType2D originalBodyType;

    private IEnumerator SequenceRoutine()
    {
        if (movementScript != null) movementScript.enabled = false;
        
        if (playerRb != null) 
        {
            originalBodyType = playerRb.bodyType;
            playerRb.linearVelocity = Vector2.zero;
        }

        if (square1 != null) playerObj.transform.position = square1.position;

        if (square2 != null)
        {
            while (Vector3.Distance(playerObj.transform.position, square2.position) > 0.1f)
            {
                Vector3 dir = (square2.position - playerObj.transform.position).normalized;

                // Yön Dönüşü
                if (movementScript != null && movementScript.GetType().Name == "PlatformerMovement")
                {
                    bool faceRight = dir.x >= 0;
                    playerObj.transform.eulerAngles = new Vector3(0, faceRight ? 0 : 180, 0);
                    SetPrivateField("facingRight", faceRight);
                }

                UpdateWalkAnimations(dir);
                playerObj.transform.position = Vector3.MoveTowards(playerObj.transform.position, square2.position, autoWalkSpeed * Time.deltaTime);
                yield return null;
            }
            playerObj.transform.position = square2.position;
            StopWalkAnimations();
        }

        SetRestrictLeftMovement(true);
        if (movementScript != null) movementScript.enabled = true;

        while (square3Trigger != null && !square3Trigger.OverlapPoint(playerObj.transform.position))
            yield return null;

        if (playerRb != null) playerRb.bodyType = originalBodyType;
        SetRestrictLeftMovement(false);
        Destroy(gameObject);
    }

    private void SetRestrictLeftMovement(bool restrict)
    {
        if (movementScript == null) return;
        var field = movementScript.GetType().GetField("restrictToRightOnly", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(movementScript, restrict);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = movementScript.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(movementScript, value);
    }

    private void UpdateWalkAnimations(Vector3 dir)
    {
        if (playerAnim == null) return;
        SetAnimFloat("Speed", 1f);
        SetAnimBool("isWalking", true);
        SetAnimFloat("InputX", dir.x);
    }

    private void StopWalkAnimations()
    {
        if (playerAnim == null) return;
        SetAnimFloat("Speed", 0f);
        SetAnimBool("isWalking", false);
    }

    private void SetAnimFloat(string n, float v) { if (HasP(n)) playerAnim.SetFloat(n, v); }
    private void SetAnimBool(string n, bool v) { if (HasP(n)) playerAnim.SetBool(n, v); }
    private bool HasP(string n)
    {
        foreach (var p in playerAnim.parameters) if (p.name == n) return true;
        return false;
    }
}