using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Auth;

public class RankPanel : MonoBehaviour
{
    [Header("Scroll View")]
    public Transform content;
    public GameObject itemRankPrefab;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private async void OnEnable()
    {
        if (UserDataManager.instance != null)
        {
            auth = UserDataManager.instance.Auth;
            dbRef = UserDataManager.instance.DbRef;
        }
        else
        {
            Debug.LogError("UserDataManager.instance is null!");
            return;
        }

        // XÓA TOÀN BỘ ITEM CŨ TRONG LIST
        foreach (Transform child in content)
            Destroy(child.gameObject);

        try
        {
            // LẤY TẤT CẢ USER TRONG "users"
            var snapshot = await dbRef.Child("users").GetValueAsync();
            List<UserEntry> list = new List<UserEntry>();

            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    string json = child.GetRawJsonValue();
                    var userData = JsonUtility.FromJson<UserDataManager.UserData>(json);

                    if (userData == null) continue;

                    // Lấy tên hiển thị (email trước dấu @)
                    string displayName = userData.email;
                    int at = displayName.IndexOf('@');
                    if (at > 0) displayName = displayName[..at];

                    list.Add(new UserEntry
                    {
                        uid = child.Key,
                        name = displayName,
                        level = userData.level,
                        exp = userData.exp
                    });
                }
            }

            // SORT: LEVEL giảm → nếu bằng thì EXP giảm
            list.Sort((a, b) =>
            {
                int lv = b.level.CompareTo(a.level);
                if (lv == 0) lv = b.exp.CompareTo(a.exp);
                return lv;
            });

            // =========================
            //  TÌM USER HIỆN TẠI
            // =========================
            string currentUid = auth != null && auth.CurrentUser != null
                ? auth.CurrentUser.UserId
                : null;

            int myIndex = -1;
            UserEntry myEntry = null;

            if (!string.IsNullOrEmpty(currentUid))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].uid == currentUid)
                    {
                        myIndex = i;
                        myEntry = list[i];
                        break;
                    }
                }
            }

            // =========================
            //  SPAWN UI
            // =========================

            // 1. HÀNG CỦA CHÍNH MÌNH LÊN ĐẦU (MÀU ĐỎ)
            if (myEntry != null)
            {
                int myRankNumber = myIndex + 1; // vì index bắt đầu từ 0
                GameObject myObj = Instantiate(itemRankPrefab, content);
                var myUI = myObj.GetComponent<RankItemUI>();

                // isSelf = true để RankItemUI biết tô BG đỏ
                myUI.Setup(myRankNumber, myEntry.name, myEntry.level, myEntry.exp, true);
            }

            // 2. CÁC USER CÒN LẠI (TRỪ MÌNH RA)
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];

                // bỏ qua user hiện tại (đã vẽ ở trên)
                if (!string.IsNullOrEmpty(currentUid) && entry.uid == currentUid)
                    continue;

                int rankNumber = i + 1;

                GameObject obj = Instantiate(itemRankPrefab, content);
                var ui = obj.GetComponent<RankItemUI>();

                // isSelf = false, các hàng thường
                ui.Setup(rankNumber, entry.name, entry.level, entry.exp, false);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Lỗi load rank: " + e.Message);
        }
    }

    private class UserEntry
    {
        public string uid;
        public string name;
        public int level;
        public int exp;
    }

    public void OnCloseButton()
    {
        UIManager.Instance.CloseOverlay(gameObject);
    }
}
