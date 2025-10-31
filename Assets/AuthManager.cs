using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        statusText.text = "Sẵn sàng để đăng nhập...";
    }

    public void Register()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Vui lòng nhập email và mật khẩu";
            return;
        }

        statusText.text = "Đang đăng ký...";

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    statusText.text = "Đăng ký thất bại!";
                    return;
                }

                statusText.text = "Đăng ký thành công: " + task.Result.User.Email;
            });
    }

    public void Login()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Vui lòng nhập email và mật khẩu";
            return;
        }

        statusText.text = "Đang đăng nhập...";

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    statusText.text = "Đăng nhập thất bại!";
                    return;
                }

                statusText.text = "Xin chào, " + task.Result.User.Email;
                UIManager.Instance.Show(UIManager.Instance.mainUI);
            });
    }
}
