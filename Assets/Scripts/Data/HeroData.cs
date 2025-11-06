using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroData", menuName = "Game/Hero Data")]
public class HeroData : ScriptableObject
{
    public string heroId;       // định danh duy nhất (vd: "naruto", "tanjiro")
    public string heroName;     // tên hiển thị
    public int level = 1;
    public int price = 0;
    public bool isUnlocked = false;
    public Sprite icon;
}
