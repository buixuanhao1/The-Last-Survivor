using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map_", menuName = "Game/Map Config")]
public class MapConfig : ScriptableObject
{
    [Header("Thông tin hiện thị")]
    public string mapid;
    public string displayName;
    public Sprite previewSprite;

    [Header("Background")]
    public Sprite backgroundSprite;

    [Header("Enemy trong map")]
    public List<EnemyType> enemyTypes;
}
