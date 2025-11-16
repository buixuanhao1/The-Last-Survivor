using UnityEngine;

public enum WeaponId
{
    Shuriken,
    Tarot,
    StoneOrbit,
    Sword,
}

public class PlayerWeaponSystem : MonoBehaviour
{
    public int shurikenCount = 0;   // phi tiêu
    public int tarotCount = 1;   // locked at start
    public int stoneCount = 0;   // locked at start
    public int swordCount = 0;   // locked at start

    private void Awake()
    {
        ResetWeapon();
    }

    public void ResetWeapon()
    {
        shurikenCount = 0;
        tarotCount = 1;
        stoneCount = 0;
        swordCount = 0;
    }

    public void SetShurikenCount(int newCount)
    {
        shurikenCount = Mathf.Max(0, newCount);
    }

    public void SetTarotCount(int newCount)
    {
        tarotCount = Mathf.Max(0, newCount);
    }

    public void SetStoneCount(int newCount)
    {
        stoneCount = Mathf.Max(0, newCount);
    }

    public void SetSwordCount(int newCount)
    {
        swordCount = Mathf.Max(0, newCount);
    }

}
