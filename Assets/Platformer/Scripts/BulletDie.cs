using UnityEngine;

public class BulletDie : MonoBehaviour
{
    public float lifeTime = 10f;
    private void Start()
    {
        Invoke(nameof(DestroyItself), lifeTime);
    }
    void DestroyItself()
    {
         Destroy(gameObject);
    }
}
