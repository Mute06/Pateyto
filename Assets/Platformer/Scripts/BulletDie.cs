using UnityEngine;

public class BulletDie : MonoBehaviour
{
    public float lifeTime = 10f;
    public GameObject bulleftHitEffect;
    private void Start()
    {
        Invoke(nameof(DestroyItself), lifeTime);
    }
    void DestroyItself()
    {
         Instantiate(bulleftHitEffect,transform.position, Quaternion.identity);
         Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {


        if (collision != null)
        {
            if (collision.CompareTag("Player") || collision.CompareTag("Enemy"))
            {
                collision.GetComponent<IDamagable>()?.TakeDamage();
                DestroyItself();
            }
            else
            {
                DestroyItself();
            }
        }
    }
}
