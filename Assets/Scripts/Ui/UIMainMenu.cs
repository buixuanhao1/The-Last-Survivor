using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIMainMenu : MonoBehaviour
{
    [Header("Init Options")]
    [Tooltip("Trong Editor: bỏ qua kiểm tra Firebase để tránh kẹt khi chưa init Firebase")] [SerializeField]
    private bool skipFirebaseCheckInEditor = true;

    [Header("Map Selection")]
    [SerializeField] private List<MapConfig> maps = new List<MapConfig>();
    [SerializeField] private Image mapPreviewImage;
    [SerializeField] private TMP_Text mapNameText;
    private int currentMapIndex = 0;

    private void Start()
    {
        // Trì hoãn 1 frame để đảm bảo UIManager đã sẵn sàng
        StartCoroutine(DeferredApply());
        InitializeMapUI();
    }

    private void OnEnable()
    {
        // Tránh gọi 2 lần liên tiếp ngay frame đầu
    }

    private void ApplyLoginStateUI()
    {
        bool loggedIn = false;
        try
        {
#if UNITY_EDITOR
            if (skipFirebaseCheckInEditor)
            {
                loggedIn = false;
            }
            else
#endif
            {
                var inst = FirebaseAuth.DefaultInstance;
                loggedIn = (inst != null && inst.CurrentUser != null);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Firebase check failed: {ex.Message}. Fallback to loggedOut.");
            loggedIn = false;
        }

        if (loggedIn)
        {
            UIManager.Instance.ShowPanel(UIManager.Instance.mainUIPrefab);
        }
        else
        {
            
            Debug.Log("Chưa đăng nhập -> cần hiển thị Login Panel");
        }
    }
    private System.Collections.IEnumerator DeferredApply()
    {
        yield return null; // đợi 1 frame để các Singleton khởi tạo
        if (UIManager.Instance == null)
        {
            // nếu vẫn chưa có UIManager, đợi thêm 1 frame
            yield return null;
        }
        ApplyLoginStateUI();
    }

    private void InitializeMapUI()
    {
        if (maps == null || maps.Count == 0) return;
        int saved = PlayerPrefs.GetInt("SelectedMapIndex", 0);
        currentMapIndex = Mathf.Clamp(saved, 0, maps.Count - 1);
        UpdateMapUI();
    }

    private void UpdateMapUI()
    {
        if (maps == null || maps.Count == 0) return;
        var map = maps[currentMapIndex];
        if (mapPreviewImage != null) mapPreviewImage.sprite = map.previewSprite;
        if (mapNameText != null) mapNameText.text = map.displayName;
        PlayerPrefs.SetInt("SelectedMapIndex", currentMapIndex);
        PlayerPrefs.Save();
        SelectMap(map);
    }

    public void NextMap()
    {
        if (maps == null || maps.Count == 0) return;
        currentMapIndex = (currentMapIndex + 1) % maps.Count;
        UpdateMapUI();
    }

    public void PrevMap()
    {
        if (maps == null || maps.Count == 0) return;
        currentMapIndex = (currentMapIndex - 1 + maps.Count) % maps.Count;
        UpdateMapUI();
    }
    public void SelectMap(MapConfig map)
    {
        if (MapSelection.Instance == null)
        {
            Debug.LogWarning("MapSelection instance not found in scene. Please add MapSelection to the Main Menu scene.");
            return;
        }
        MapSelection.Instance.Selected = map;
    }

    public void OpenUserProfile()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.profilePanelPrefab);
    }

    public void OpenShop()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.shopPanelPrefab);
    }

    public void OpenHero()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.heroPanelPrefab);
    }
    public void OpenSetting()
    {
        UIManager.Instance.OpenOverlay(UIManager.Instance.settingsPanelPrefab);
    }

    public void OpenRankPanle()
    {
        UIManager.Instance.OpenOverlay(UIManager.Instance.rankPanlePrefab);
    }

    public void OpenUpgradeHeroPanel()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.upgradeHeroPrefab);
    }

    public void OpenPetUIPanel()
    {
        UIManager.Instance.ShowPanel(UIManager.Instance.petPrefab);
    }

    public void LoadGamePlay()
    {
        bool loggedIn = FirebaseAuth.DefaultInstance != null &&
                        FirebaseAuth.DefaultInstance.CurrentUser != null;

        if (!loggedIn)
        {
            UIManager.Instance.ShowPanel(UIManager.Instance.loginPanelPrefab);
            return;
        }

        SceneManager.LoadScene("GamePlay");
    }

}
