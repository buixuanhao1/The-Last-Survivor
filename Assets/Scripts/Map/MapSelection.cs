using UnityEngine;

public class MapSelection : MonoBehaviour
{
    public static MapSelection Instance;
    public MapConfig Selected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
