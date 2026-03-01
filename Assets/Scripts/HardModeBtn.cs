using UnityEngine;

public class HardModeBtn : MonoBehaviour
{

    public GameObject hardGameBtn;
    private void Start()
    {
        if (PlayerPrefs.GetInt("HardGame", 0) == 1)
        {
            hardGameBtn.SetActive(true);
        }
        else
        {                       
            hardGameBtn.SetActive(false);

        }
    }

    public void LoadShooterScene()
    {
        P_SceneManager.Instance.LoadLevelIndexWithFade(8, 1f);
    }
}
