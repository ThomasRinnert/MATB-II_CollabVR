using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StressOverTime : MonoBehaviour
{
    [SerializeField] Operator op;
    [SerializeField] TextMeshPro stressHistory;
    [SerializeField] StressOverTimeGraph graph;
    
    [Range(0.0f, 10.0f)][Tooltip("Number of history item to display")]
    [SerializeField] int LastLevelsCount = 3;

    private Operator.StressLevel lastStressLevel = Operator.StressLevel.Low;
    private string current = "";
    private string state   = "";
    private string history = "";

    void Start()
    {
        if (graph == null) graph = GetComponentInChildren<StressOverTimeGraph>();
    }

    void Update()
    {
        string level = "ERROR"; string color = "white";
        if      (op.stressLevel == Operator.StressLevel.High)   {level = "HIGH";   color = ColorUtility.ToHtmlStringRGBA(graph.colors[0]);}
        else if (op.stressLevel == Operator.StressLevel.Medium) {level = "MEDIUM"; color = ColorUtility.ToHtmlStringRGBA(graph.colors[1]);}
        else if (op.stressLevel == Operator.StressLevel.Low)    {level = "LOW";    color = ColorUtility.ToHtmlStringRGBA(graph.colors[2]);}

        float duration = 0.0f;
        for (int i = 0; i < op.stressOverTime.Count; i++)
        {
            duration += op.stressOverTime[i].Item2;
        }

        current = "Stress value: <color=#" + color + ">" + (op.stress*100).ToString("##0'%'") + "</color> " + (op.isWorking() ? "(increasing)" : "(decreasing)");
        state = "\nHas been " + level + " for " + duration.ToString("###0's'");

        if (lastStressLevel != op.stressLevel)
        {
            lastStressLevel = op.stressLevel;
        }

        history = "";
        for (int i = 0; i < LastLevelsCount && i <op.stressRecords.Count; i++)
        {
            Operator.StressRecord record = op.stressRecords[op.stressRecords.Count-1-i];

            /*
            level = "ERROR";
            if (record.level == Operator.StressLevel.Low) level = "LOW";
            else if (record.level == Operator.StressLevel.Medium) level = "MEDIUM";
            else if (record.level == Operator.StressLevel.High) level = "HIGH";
            */
            
            history += "\nWas " + record.level.String() + /*" (Avg: " + (record.average * 100).ToString("##0'%)'") + */" for " + record.duration.ToString("##0's'") + " before that";
        }

        stressHistory.text = current + state + history;
    }
}
