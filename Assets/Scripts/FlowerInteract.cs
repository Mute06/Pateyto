using FirstPersonSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlowerInteract : MonoBehaviour
{
    public UnityEvent[] OnInteractEvents;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the flower's trigger area.");
            foreach (var unityEvent in OnInteractEvents)
            {
                unityEvent.Invoke();
            }
            P_SceneManager.Instance.LoadNextLevelWithFade(3f);
        }
    }


}
