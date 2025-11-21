using UnityEngine;

public class CameraWalls : MonoBehaviour
{
    public Camera cam;

    public BoxCollider2D leftWall;
    public BoxCollider2D rightWall;
    public BoxCollider2D topWall;
    public BoxCollider2D bottomWall;

    public float thickness = 1f;   // độ dày tường

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;
        Vector3 c = cam.transform.position;

        // set size
        leftWall.size = new Vector2(thickness, halfHeight * 2f);
        rightWall.size = new Vector2(thickness, halfHeight * 2f);
        topWall.size = new Vector2(halfWidth * 2f, thickness);
        bottomWall.size = new Vector2(halfWidth * 2f, thickness);

        // set vị trí ngay ngoài viền màn hình 1 nửa thickness
        leftWall.transform.position = new Vector3(c.x - halfWidth - thickness * 0.5f, c.y, 0);
        rightWall.transform.position = new Vector3(c.x + halfWidth + thickness * 0.5f, c.y, 0);
        topWall.transform.position = new Vector3(c.x, c.y + halfHeight + thickness * 0.5f, 0);
        bottomWall.transform.position = new Vector3(c.x, c.y - halfHeight - thickness * 0.5f, 0);
    }
}
