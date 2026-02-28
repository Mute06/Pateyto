using UnityEngine;

public class BloodVisual : MonoBehaviour
{
    public bool isBackgroundSplash;
    public bool isGroundSplat;
    public Sprite[] randomSprites;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Randomly rotate to add variety
            transform.Rotate(0, 0, Random.Range(0f, 360f));
            
            // Randomize color slightly
            float darkVal = Random.Range(0.6f, 1f);
            sr.color = new Color(darkVal, darkVal, darkVal, 1f);
            
            if (randomSprites != null && randomSprites.Length > 0)
            {
                sr.sprite = randomSprites[Random.Range(0, randomSprites.Length)];
            }
        }
        
        // Clean up eventually to avoid memory leak
        Destroy(gameObject, 30f);
    }
}
