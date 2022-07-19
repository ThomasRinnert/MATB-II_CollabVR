using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskSpawning : MonoBehaviour
{
    public int taskNumber = 2;
    public GameObject taskTable;
    public List<GameObject> taskPrefabs = new List<GameObject>();
    
    void Update()
    {
        if (transform.childCount < taskNumber)
        {
            GameObject task = Instantiate(taskPrefabs[Random.Range(0, taskPrefabs.Count)], transform.position, Quaternion.identity);
            task.transform.SetParent(transform);
        }
    }
}
