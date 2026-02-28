using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cinemachineCamera = GetComponent<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            cinemachineBasicMultiChannelPerlin = cinemachineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            // Fix: reset amplitude gain to 0 at start so it doesn't shake indefinitely if the component's default is > 0
            if (cinemachineBasicMultiChannelPerlin != null)
            {
                cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
            }
        }
    }

    /// <summary>
    /// Call this method to shake the camera. 
    /// Example: CameraShake.Instance.ShakeCamera(5f, 0.5f);
    /// </summary>
    /// <param name="intensity">Amplitude Gain of the shake</param>
    /// <param name="time">Duration of the shake in seconds</param>
    public void ShakeCamera(float intensity, float time)
    {
        if (cinemachineBasicMultiChannelPerlin != null)
        {
            cinemachineBasicMultiChannelPerlin.AmplitudeGain = intensity;

            startingIntensity = intensity;
            shakeTimerTotal = time;
            shakeTimer = time;
        }
        else
        {
            Debug.LogWarning("CameraShake: No CinemachineBasicMultiChannelPerlin component found on this camera.");
        }
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0f)
            {
                // Timer over!
                if (cinemachineBasicMultiChannelPerlin != null)
                {
                    cinemachineBasicMultiChannelPerlin.AmplitudeGain = 0f;
                }
            }
            else
            {
                if (cinemachineBasicMultiChannelPerlin != null)
                {
                    // Lerp amplitude down over time
                    cinemachineBasicMultiChannelPerlin.AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1f - (shakeTimer / shakeTimerTotal));
                }
            }
        }
    }
}
