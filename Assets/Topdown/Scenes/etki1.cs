using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class etji1 : MonoBehaviour
{
    public string text;
    public TextMeshProUGUI a;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("yes");
            if (a != null)
            {
                a.text = text;
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component 'a' is not assigned.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("no");
            if (a != null)
            {
                a.text = "";
            }
            else
            {
                Debug.LogError("TextMeshProUGUI component 'a' is not assigned.");
            }
        }
    }
}
