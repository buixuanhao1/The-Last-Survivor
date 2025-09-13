using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] public GameObject prefab;
    [SerializeField] private int initialSize = 10;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Start()
    {
        for(int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab);
            pool.Enqueue(obj);
            obj.SetActive(false);
        }
    }

    public GameObject Get(Vector3 postion, Quaternion rotation)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj= pool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab);
        }
        obj.transform.position = postion;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    } 

}
