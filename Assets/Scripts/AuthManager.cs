using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;


public class AuthManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text statusText;

    private FirebaseAuth auth;

    void Start()
    {
        StartCoroutine(InitFirebaseAuth());
    }

    private IEnumerator InitFirebaseAuth()
    {
        statusText.text = " Đang khởi tạo Firebase...";
        yield return new WaitUntil(() => FirebaseInitializer.IsReady);

        auth = FirebaseAuth.DefaultInstance;
        statusText.text = " Sẵn sàng để đăng nhập...";
    }

    public void Register()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = " Vui lòng nhập email và mật khẩu";
            return;
        }

        statusText.text = " Đang đăng ký...";

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    statusText.text = " Đăng ký thất bại!";
                    return;
                }

                FirebaseUser user = task.Result.User;
                statusText.text = " Đăng ký thành công: " + user.Email;

                // Tạo dữ liệu mới cho người dùng
                UserDataManager.UserData newData = new UserDataManager.UserData(user.Email);
                UserDataManager.instance.SaveUserData(newData);
            });
    }

    public void Login()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Vui lòng nhập email và mật khẩu.";
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            statusText.text = "Email không hợp lệ.";
            return;
        }

        statusText.text = "Đang đăng nhập...";

        auth.SignInWithEmailAndPasswordAsync(email, password)
        .ContinueWithOnMainThread(async task =>
        {
            // Nếu bị hủy
            if (task.IsCanceled)
            {
                statusText.text = "Đăng nhập bị hủy.";
                return;
            }

            // Nếu lỗi
            if (task.IsFaulted)
            {
                string errorMsg = "Đăng nhập thất bại nha. Kiểm tra lại email hoặc mật khẩu.";
                statusText.text = errorMsg;
                return;
            }

            //  Thành công
            try
            {
                FirebaseUser user = task.Result.User;
                statusText.text = "Đăng nhập thành công: " + user.Email;
                Debug.Log("Đăng nhập thành công: " + user.Email);

                var data = await UserDataManager.instance.LoadUserData();
                if (data != null)
                {
                    Debug.Log($"Dữ liệu tải về: Gold={data.gold}, Diamond={data.diamond}, Level={data.level}");
                }

                UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Lỗi khi tải dữ liệu người dùng: " + ex.Message);
                statusText.text = "Đăng nhập thất bại (lỗi khi tải dữ liệu).";
            }
        });
    }


}
