using UnityEngine;


public class InfiniteBackground : MonoBehaviour
{
    public Transform player;         // Nhân vật
    public SpriteRenderer bgPrefab;  // Prefab background

    private float tileWidth;
    private float tileHeight;
    private Transform[,] tiles = new Transform[3, 3]; // 9 tile xung quanh

    void Start()
    {
        // Lấy kích thước world unit của background
        tileWidth = bgPrefab.bounds.size.x;
        tileHeight = bgPrefab.bounds.size.y;

        // Tạo 9 tile (3x3) bao quanh
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 pos = new Vector3(x * tileWidth, y * tileHeight, 0);
                Transform t = Instantiate(bgPrefab, pos, Quaternion.identity, transform).transform;
                tiles[x + 1, y + 1] = t;
            }
        }
    }

    void Update()
    {
        // Tính tile trung tâm dựa theo vị trí player
        int centerX = Mathf.RoundToInt(player.position.x / tileWidth);
        int centerY = Mathf.RoundToInt(player.position.y / tileHeight);

        // Cập nhật vị trí các tile
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                // Snap để tránh sai số float gây kẻ đường
                float newX = Mathf.Round((centerX + x) * tileWidth * 100f) / 100f;
                float newY = Mathf.Round((centerY + y) * tileHeight * 100f) / 100f;

                Vector3 pos = new Vector3(newX, newY, 0);
                tiles[x + 1, y + 1].position = pos;
            }
        }
    }
}
