using UnityEngine;

public class LightLookAt2D : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The transform this light should point its transform.up towards.")]
    public Transform target;

    private void Update()
    {
        if (target != null)
        {
            // Get the direction from this object to the target
            Vector3 direction = target.position - transform.position;
            
            // Neutralize the Z axis to ensure it's strictly a 2D rotation
            direction.z = 0f;

            if (direction != Vector3.zero)
            {
                // Align the transform.up vector with the direction vector
                transform.up = direction;
            }
        }
    }
}
