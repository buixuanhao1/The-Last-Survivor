using UnityEngine;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Loại Enemy có thể spawn")]
    public EnemyType[] enemyTypes;   

    [Header("Thông số spawn")]
    public Transform player;
    public float spawnRate = 2f;     // thời gian giữa 2 lần spawn
    public float spawnRadius = 10f;  // khoảng cách spawn so với player

    private float timer;

    void Start()
    {
        if (MapSelection.Instance != null && MapSelection.Instance.Selected != null)
        {
            var list = MapSelection.Instance.Selected.enemyTypes;
            if (list != null && list.Count > 0)
            {
                enemyTypes = list.ToArray();
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        // random EnemyData
        EnemyType type = enemyTypes[Random.Range(0, enemyTypes.Length)];

        // random vị trí spawn quanh player
        Vector2 spawnPos = (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;

        // tạo enemy từ prefab
        GameObject enemy = Instantiate(type.prefab, spawnPos, Quaternion.identity);

        // gán data cho controller
        var controller = enemy.GetComponent<EnemyController>();
        controller.enemyData = type.data;
    }
}
