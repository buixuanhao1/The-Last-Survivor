using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool IsReady = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var status = task.Result;
            if (status == DependencyStatus.Available)
            {
                IsReady = true;
                Debug.Log(" Firebase đã sẵn sàng!");
            }
            else
            {
                Debug.LogError($" Firebase chưa sẵn sàng: {status}");
            }
        });
    }
}
