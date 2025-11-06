using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public UserData currentData;
    public static UserDataManager instance;
    private DatabaseReference dbRef;
    private FirebaseAuth auth;
    public int MaxEnergy = 40;
    public int EnergyRegenTime = 300; // 5 phút / 1 energy
    [System.Serializable]
    public class UserData
    {
        public string email;
        public int gold;
        public int diamond;
        public int level;
        public int energy;
        public long lastEnergyUpdate; // timestamp
        public string selectedHero;
        public List<string> unlockedHeroes;

        public UserData() { }

        public UserData(string email)
        {
            this.email = email;
            this.gold = 1000;
            this.diamond = 500;
            this.level = 1;
            this.energy = 40; // full energy
            this.lastEnergyUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.selectedHero = "Default";
            this.unlockedHeroes = new List<string>() { "Naruto" }; // hero mặc định có sẵn

        }
    }
    public void UpdateEnergy()
    {
        var data = currentData;
        if (data == null) return;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long diff = now - data.lastEnergyUpdate;

        int energyToRegen = (int)(diff / EnergyRegenTime);
        if (energyToRegen > 0 && data.energy < MaxEnergy)
        {
            data.energy = Math.Min(MaxEnergy, data.energy + energyToRegen);
            data.lastEnergyUpdate = now;
            SaveUserData(data);
        }
    }
    private async void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Kiểm tra và khởi tạo Firebase
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            var app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;

            dbRef = FirebaseDatabase.GetInstance(app,
                "https://the-last-survivor-5ad55-default-rtdb.asia-southeast1.firebasedatabase.app"
            ).RootReference;

            Debug.Log(" Firebase Database đã sẵn sàng!");
        }
        else
        {
            Debug.LogError($" Firebase dependencies not resolved: {dependencyStatus}");
        }
    }

    public async void SaveUserData(UserData data)
    {
        string uid = auth.CurrentUser.UserId;
        string json = JsonUtility.ToJson(data);
        await dbRef.Child("users").Child(uid).SetRawJsonValueAsync(json);
    }

    public async Task<UserData> LoadUserData()
    {
        string uid = auth.CurrentUser.UserId;
        var snapshot = await dbRef.Child("users").Child(uid).GetValueAsync();
        if (snapshot.Exists)
        {
            string json = snapshot.GetRawJsonValue();
            currentData = JsonUtility.FromJson<UserData>(json);
            return currentData;
        }
        else
        {
            var newData = new UserData(auth.CurrentUser.Email);
            currentData = newData; 
            SaveUserData(newData);
            return newData;
        }
    }



}
