using UnityEngine;

public class TutorialAutoHide : MonoBehaviour
{
    public float hideAfterSeconds = 10f;

    void Start()
    {
        Destroy(gameObject, hideAfterSeconds);
    }
}