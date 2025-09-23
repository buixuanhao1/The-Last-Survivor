using System.Collections.Generic;
using UnityEngine;

public class ExpOrbPool : MonoBehaviour
{
    public static ExpOrbPool Instance;

    public GameObject orbPrefab;
    public int poolSize = 100;

    private Queue<ExpOrb> pool = new Queue<ExpOrb>();
    private Transform player;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Tạo sẵn pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(orbPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj.GetComponent<ExpOrb>());
        }
    }

    public void SpawnOrb(Vector3 position, int expAmount)
    {
        ExpOrb orb;
        if (pool.Count > 0)
        {
            orb = pool.Dequeue();
        }
        else
        {
            // Nếu pool hết, tạo mới (hoặc bạn có thể chặn)
            orb = Instantiate(orbPrefab).GetComponent<ExpOrb>();
        }

        orb.transform.position = position;
        orb.Init(expAmount, player);
    }

    public void ReturnToPool(ExpOrb orb)
    {
        orb.gameObject.SetActive(false);
        pool.Enqueue(orb);
    }
}
