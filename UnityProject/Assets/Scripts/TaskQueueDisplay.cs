using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskQueueDisplay : MonoBehaviour
{
    [SerializeField] GameObject SYSMON;
    private TextMeshPro SYSMON_txt;
    private Image SYSMON_circle = null;

    [SerializeField] GameObject TRACK;
    private TextMeshPro TRACK_txt;
    private Image TRACK_circle = null;

    [SerializeField] GameObject COMM;
    private TextMeshPro COMM_txt;
    private Image COMM_circle = null;

    [SerializeField] GameObject RESMAN;
    private TextMeshPro RESMAN_txt;
    private Image RESMAN_circle = null;

    [SerializeField] Operator op;

    [ContextMenu("Bind")]
    void Bind()
    {
        if (SYSMON_txt == null) SYSMON_txt = SYSMON.GetComponentInChildren<TextMeshPro>();
        if (SYSMON_circle == null) SYSMON_circle = SYSMON.GetComponentInChildren<Image>();
        if (TRACK_txt == null) TRACK_txt = TRACK.GetComponentInChildren<TextMeshPro>();
        if (TRACK_circle == null) TRACK_circle = TRACK.GetComponentInChildren<Image>();
        if (COMM_txt == null) COMM_txt = COMM.GetComponentInChildren<TextMeshPro>();
        if (COMM_circle == null) COMM_circle = COMM.GetComponentInChildren<Image>();
        if (RESMAN_txt == null) RESMAN_txt = RESMAN.GetComponentInChildren<TextMeshPro>();
        if (RESMAN_circle == null) RESMAN_circle = RESMAN.GetComponentInChildren<Image>();
    }

    void Start()
    {
        Bind();

        SYSMON.SetActive(false);
        TRACK.SetActive(false);
        COMM.SetActive(false);
        RESMAN.SetActive(false);
    }

    void Update()
    {
        if (op.SYSMON_todo > 0 && !SYSMON.activeSelf) SYSMON.SetActive(true);
        if (op.TRACK_todo  > 0 && !TRACK.activeSelf)  TRACK.SetActive(true);
        if (op.COMM_todo   > 0 && !COMM.activeSelf)   COMM.SetActive(true);
        if (op.RESMAN_todo > 0 && !RESMAN.activeSelf) RESMAN.SetActive(true);
        
        if(op.replayer.state != Replayer.ReplayState.Playing && op.replayer.state != Replayer.ReplayState.Paused)
        {
            if (op.SYSMON_todo <= 0 && SYSMON.activeSelf) SYSMON.SetActive(false);
            if (op.TRACK_todo  <= 0 && TRACK.activeSelf)  TRACK.SetActive(false); 
            if (op.COMM_todo   <= 0 && COMM.activeSelf)   COMM.SetActive(false); 
            if (op.RESMAN_todo <= 0 && RESMAN.activeSelf) RESMAN.SetActive(false);
        }

        if (SYSMON.activeSelf)
        {
            SYSMON_txt.text = op.SYSMON_todo.ToString();
            SYSMON_circle.fillAmount = op.replayer.TASK == MATBIISystem.MATBII_TASK.SYSMON ? op.replayer.getProgress() : 0.0f;
        }
        
        if (TRACK.activeSelf)
        {
            TRACK_txt.text = op.TRACK_todo.ToString();
            TRACK_circle.fillAmount = op.replayer.TASK == MATBIISystem.MATBII_TASK.TRACK ? op.replayer.getProgress() : 0.0f;
        }

        if (COMM.activeSelf)
        {
            COMM_txt.text = op.COMM_todo.ToString();
            COMM_circle.fillAmount = op.replayer.TASK == MATBIISystem.MATBII_TASK.COMM ? op.replayer.getProgress() : 0.0f;
        }
        
        if (RESMAN.activeSelf)
        {
            RESMAN_txt.text = op.RESMAN_todo.ToString();
            RESMAN_circle.fillAmount = op.replayer.TASK == MATBIISystem.MATBII_TASK.RESMAN ? op.replayer.getProgress() : 0.0f;
        }
    }
}
