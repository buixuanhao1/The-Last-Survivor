using UnityEngine;

[CreateAssetMenu(menuName = "Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Thông tin cơ bản")]
    public string enemyName;
    public int baseHP;
    public float moveSpeed;
    public int damage;
    public int expDrop;

    [Header("Hình ảnh")]
    public Sprite sprite;
}
