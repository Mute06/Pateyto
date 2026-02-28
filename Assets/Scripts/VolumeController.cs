using UnityEngine;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public AudioMixer mixer;

    public void SetVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        mixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
    }
}