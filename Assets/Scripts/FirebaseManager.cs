using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("🔹 Bắt đầu khởi tạo Firebase...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var status = task.Result;
            Debug.Log("🔸 Dependency status: " + status);

            if (status == DependencyStatus.Available)
            {
                FirebaseApp app = FirebaseApp.DefaultInstance;
            }
            else
            {
                Debug.LogError("Firebase chưa sẵn sàng: " + status);
            }
        });
    }
}
