using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuController : MonoBehaviour
{
    // Hàm này sẽ gọi khi nhấn nút Play
    public void PlayGame()
    {
        SceneManager.LoadScene("GamePlay");
    }
}
