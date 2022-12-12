using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskSpawning : MonoBehaviour
{
    public ExperimentControlsMATBIITeam control = null;
    public TaskSchedule schedule = null;
    public int index = 0;
    public bool started = false;

    public int taskNumber = 2;
    public List<GameObject> taskPrefabs = new List<GameObject>();

    void Start()
    {
        if (control == null) control = ExperimentControlsMATBIITeam.Instance;
        if (schedule != null)
        {
            // Check the schedule is sorted
            schedule.Sort();

            // Destroy the default tasks
            for (int c = 0; c < transform.childCount; c++) { Destroy(transform.GetChild(c).gameObject); }
        }
        ResetSpawner();
    }

    public void ResetSpawner()
    {
        index = 0;
        started = false;
    }

    public void StartSchedule()
    {
        ResetSpawner();
        started = true;
    }
    
    void Update()
    {
        if (schedule == null)
        {
            if (transform.childCount < taskNumber) { SpawnTask(); }
        }
        else if (started && index < schedule.IncomingTasks.Count)
        {
            if (control.getElapsedTime().TotalSeconds > schedule.IncomingTasks[index].time)
            {
                int count = schedule.IncomingTasks[index].numberOfTask;
                SpawnTask(count);
                control.LogTaskCreated(count);
                index++;
            }
            if (index >= schedule.IncomingTasks.Count) ResetSpawner();
        }
    }

    public void SpawnTask(int count = 1)
    {
        StartCoroutine(SpawnTaskCoroutine(count));
    }

    public void SpawnTask(MATBIISystem.MATBII_TASK task, int count = 1)
    {
        GameObject taskArtifact = null;
        for (int t = 0; t < taskPrefabs.Count; t++)
        {
            TaskArtifact artifact = taskPrefabs[t].GetComponent<TaskArtifact>();
            if (artifact.task == task) taskArtifact = taskPrefabs[t];
        }
        if (taskArtifact == null) { Debug.LogError("TaskSpawning: Given task not found among prefab list"); return; }

        StartCoroutine(SpawnTaskCoroutine(count, taskArtifact));
    }

    IEnumerator SpawnTaskCoroutine(int count, GameObject task = null)
    {
        while(count > 0)
        {
            float x = ((count % 3) - 1) * (0.2f + UnityEngine.Random.Range(-0.1f, 0.1f));
            Vector3 spawnpos = new Vector3(transform.position.x + x, transform.position.y, transform.position.z);

            GameObject spawnedTask = Instantiate((task != null) ? task : taskPrefabs[UnityEngine.Random.Range(0, taskPrefabs.Count)], spawnpos, Quaternion.identity);
            spawnedTask.transform.SetParent(transform);
            //control.LogTaskCreated(spawnedTask.GetComponent<TaskArtifact>().task);
            
            count--;
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
