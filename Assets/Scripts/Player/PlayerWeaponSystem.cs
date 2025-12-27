using UnityEngine;

public enum WeaponId
{
    Shuriken,
    Tarot,
    StoneOrbit,
    Sword,
    Knife,
    ReturningShuriken,
    Bomb,
}

public class PlayerWeaponSystem : MonoBehaviour
{
    public int shurikenCount = 1;   // phi tiêu
    public int tarotCount = 0;   // locked at start
    public int stoneCount = 0;   // locked at start
    public int swordCount = 0;   // locked at start
    public int knifeCount = 0;   // locked at start
    public int returningShurikenCount = 0; // locked at start
    public int bombCount = 0; // locked at start

    private void Awake()
    {
        ResetWeapon();
    }

    public void ResetWeapon()
    {
        shurikenCount = 1;
        tarotCount = 0;
        stoneCount = 0;
        swordCount = 0;
        knifeCount = 0;
        returningShurikenCount = 0;
        bombCount = 0;
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

    public void SetKnifeCount(int newCount)
    {
        knifeCount = Mathf.Max(0, newCount);
    }

    public void SetReturningShurikenCount(int newCount)
    {
        returningShurikenCount = Mathf.Max(0, newCount);
    }

    public void SetBombCount(int newCount)
    {
        bombCount = Mathf.Max(0, newCount);
    }

}
