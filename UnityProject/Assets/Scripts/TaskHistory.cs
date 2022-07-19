using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskHistory : MonoBehaviour
{
    [SerializeField] Operator op;

    [SerializeField] TextMeshPro SYSMON; 
    [SerializeField] TextMeshPro TRACK; 
    [SerializeField] TextMeshPro COMM; 
    [SerializeField] TextMeshPro RESMAN; 

    void Start()
    {
        // ...
    }

    void Update()
    {
        if (SYSMON.text != op.SYSMON_done.ToString()) SYSMON.text = op.SYSMON_done.ToString();
        if (TRACK.text != op.TRACK_done.ToString()) TRACK.text = op.TRACK_done.ToString();
        if (COMM.text != op.COMM_done.ToString()) COMM.text = op.COMM_done.ToString();
        if (RESMAN.text != op.RESMAN_done.ToString()) RESMAN.text = op.RESMAN_done.ToString();
    }
}
