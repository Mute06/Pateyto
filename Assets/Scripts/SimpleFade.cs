using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class SimpleFade : MonoBehaviour
{
    public float fadeSpeed = 2f;
    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake() => canvasGroup = GetComponent<CanvasGroup>();

    private void OnEnable()
    {
        canvasGroup.alpha = 0f;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(1f));
    }

    public void FadeOutAndDisable()
    {
        if (gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(0f, true));
        }
    }

    private IEnumerator FadeRoutine(float targetAlpha, bool disableAfter = false)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        if (disableAfter) gameObject.SetActive(false);
    }
}