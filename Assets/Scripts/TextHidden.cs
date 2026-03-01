using UnityEngine;
using System.Collections;

public class DelayedActivator : MonoBehaviour
{
    public GameObject targetObject;   // Açılacak obje
    public float waitTime = 18f;

    void Start()
    {
        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Target Object atanmadı!");
            yield break;
        }

        // Başta kapalı
        targetObject.SetActive(false);

        // Gerçek zaman bekleme (timeScale'den etkilenmez)
        yield return new WaitForSecondsRealtime(waitTime);

        // Aç
        targetObject.SetActive(true);
    }
}