using UnityEngine;


public class PlayerExperience : MonoBehaviour
{
    public int level = 1;
    public int currentExp = 0;
    public int expToNextLevel = 10;

    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log("EXP: " + currentExp + "/" + expToNextLevel);

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f); // tăng dần EXP

        Debug.Log("LEVEL UP! Level: " + level);
        // TODO: Gọi mở UI nâng cấp ở đây
    }
}
