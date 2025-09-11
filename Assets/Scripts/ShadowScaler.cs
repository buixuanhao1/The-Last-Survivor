using UnityEngine;

public class ShadowScaler : MonoBehaviour
{
    public Transform target;  // Player hoặc Enemy
    public Vector3 baseScale = new Vector3(0.3823f, 0.3506f, 1f);

    void Update()
    {
        if (target != null)
        {
            float scaleFactor = target.localScale.x;
            transform.localScale = baseScale * scaleFactor;
        }
    }
}
