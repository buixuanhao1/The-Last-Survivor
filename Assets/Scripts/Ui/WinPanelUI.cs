using UnityEngine;
using UnityEngine.SceneManagement;

public class WinPanelUI : MonoBehaviour
{
    public void OnClick_Quit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
