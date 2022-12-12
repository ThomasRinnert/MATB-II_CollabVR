using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TaskScheduleElement
{
    public int time = 0;
    public int numberOfTask = 0;
}

[CreateAssetMenu(fileName = "TaskSchedule", menuName = "ScriptableObjects/TaskSchedule", order = 1)]
public class TaskSchedule : ScriptableObject
{
    public List<TaskScheduleElement> IncomingTasks;

    [ContextMenu("SORT")]
    public void Sort()
    {
        IncomingTasks.Sort((t1,t2)=>t1.time.CompareTo(t2.time));
    }

    public void AddABatch()
    {
        IncomingTasks.Add(new TaskScheduleElement());
    }

    public (int,float) Stats()
    {
        int count = 0;
        int last = 0;
        int lastCount = 0;
        foreach (var item in IncomingTasks)
        {
            if (item.time > last)
            {
                last = item.time;
                lastCount = item.numberOfTask;
            } 
            count += item.numberOfTask;
        }

        float res1 = last + (lastCount * 12.0f) / 6.0f;
        float res2 = count * 2.0f; //* 12.0f / 6.0f;
        return (count, Mathf.Max(res1, res2));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TaskSchedule)), CanEditMultipleObjects]
public class TaskScheduleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TaskSchedule script = (TaskSchedule)target;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();

        (int, float) stats = script.Stats();
        GUILayout.Label("Total task count: " + stats.Item1.ToString());
        GUILayout.Label("Estimated duration: " + stats.Item2.ToString("##0's'"));
        
        GUILayout.EndVertical();
        
        GUILayout.BeginVertical();
        
        if(GUILayout.Button("Add a batch"))
        {
            script.AddABatch();
        }
        if(GUILayout.Button("Sort"))
        {
            script.Sort();
        }
        
        GUILayout.EndVertical();
        
        GUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
#endif