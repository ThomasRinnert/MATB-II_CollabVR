using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] public Operator op;
    bool block = true;

    public float SYSMON_meanTime = 0.0f;
    public float SYSMON_minTime = 1000.0f;
    public float SYSMON_maxTime = 0.0f;
    private float SYSMON_sum = 0.0f;
    private float SYSMON_timer = 0.0f;
    
    public float TRACK_meanTime = 0.0f;
    public float TRACK_minTime = 1000.0f;
    public float TRACK_maxTime = 0.0f;
    private float TRACK_sum = 0.0f;
    private float TRACK_timer = 0.0f;

    public float COMM_meanTime = 0.0f;
    public float COMM_minTime = 1000.0f;
    public float COMM_maxTime = 0.0f;
    private float COMM_sum = 0.0f;
    private float COMM_timer = 0.0f;
    
    public float RESMAN_meanTime = 0.0f;
    public float RESMAN_minTime = 1000.0f;
    public float RESMAN_maxTime = 0.0f;
    private float RESMAN_sum = 0.0f;
    private float RESMAN_timer = 0.0f;

    System.DateTime startTime;

    void Start()
    {
        //
    }

    [ContextMenu("DEBUG")]
    public void DEBUG()
    {
        //
        StartCoroutine(coroutine());
    }

    IEnumerator coroutine()
    {
        SYSMON_sum = 0.0f;
        for (int i = 0; i < op.replayer.defaultRecords.SYSMON_Records.Count; i++)
        {
            print("SYSMON..."); block = true; op.animator.SetBool("IK", true); op.vRIK.enabled = true;
            
            startTime = System.DateTime.UtcNow;
            op.replayer.Play(MATBIISystem.MATBII_TASK.SYSMON, Replayer.ReplaySource.DefaultRecords, i, onReplayEnded);
            
            while(block) {yield return new WaitForSecondsRealtime(0.1f);}
            SYSMON_timer = (float)((System.DateTime.UtcNow - startTime).TotalSeconds);
            SYSMON_sum += SYSMON_timer;
            if (SYSMON_timer < SYSMON_minTime) SYSMON_minTime = SYSMON_timer;
            if (SYSMON_timer > SYSMON_maxTime) SYSMON_maxTime = SYSMON_timer;
        }
        SYSMON_meanTime = SYSMON_sum / op.replayer.defaultRecords.SYSMON_Records.Count;
        TRACK_sum = 0.0f;
        for (int i = 0; i < op.replayer.defaultRecords.TRACK_Records.Count; i++)
        {
            print("TRACK..."); block = true; op.animator.SetBool("IK", true); op.vRIK.enabled = true;
            
            startTime = System.DateTime.UtcNow;
            op.replayer.Play(MATBIISystem.MATBII_TASK.TRACK, Replayer.ReplaySource.DefaultRecords, i, onReplayEnded);
            
            while(block) {yield return new WaitForSecondsRealtime(0.1f);}
            TRACK_timer = (float)((System.DateTime.UtcNow - startTime).TotalSeconds);
            TRACK_sum += TRACK_timer;
            if (TRACK_timer < TRACK_minTime) TRACK_minTime = TRACK_timer;
            if (TRACK_timer > TRACK_maxTime) TRACK_maxTime = TRACK_timer;
        }
        TRACK_meanTime = TRACK_sum / op.replayer.defaultRecords.TRACK_Records.Count;
        COMM_sum = 0.0f;
        for (int i = 0; i < op.replayer.defaultRecords.COMM_Records.Count; i++)
        {
            print("COMM..."); block = true; op.animator.SetBool("IK", true); op.vRIK.enabled = true;
            
            startTime = System.DateTime.UtcNow;
            op.replayer.Play(MATBIISystem.MATBII_TASK.COMM, Replayer.ReplaySource.DefaultRecords, i, onReplayEnded);
            
            while(block) {yield return new WaitForSecondsRealtime(0.1f);}
            COMM_timer = (float)((System.DateTime.UtcNow - startTime).TotalSeconds);
            COMM_sum += COMM_timer;
            if (COMM_timer < COMM_minTime) COMM_minTime = COMM_timer;
            if (COMM_timer > COMM_maxTime) COMM_maxTime = COMM_timer;
        }
        COMM_meanTime = COMM_sum / op.replayer.defaultRecords.COMM_Records.Count;
        RESMAN_sum = 0.0f;
        for (int i = 0; i < op.replayer.defaultRecords.RESMAN_Records.Count; i++)
        {
            print("RESMAN..."); block = true; op.animator.SetBool("IK", true); op.vRIK.enabled = true;
            
            startTime = System.DateTime.UtcNow;
            op.replayer.Play(MATBIISystem.MATBII_TASK.RESMAN, Replayer.ReplaySource.DefaultRecords, i, onReplayEnded);
            
            while(block) {yield return new WaitForSecondsRealtime(0.1f);}
            RESMAN_timer = (float)((System.DateTime.UtcNow - startTime).TotalSeconds);
            RESMAN_sum += RESMAN_timer;
            if (RESMAN_timer < RESMAN_minTime) RESMAN_minTime = RESMAN_timer;
            if (RESMAN_timer > RESMAN_maxTime) RESMAN_maxTime = RESMAN_timer;
        }
        RESMAN_meanTime = RESMAN_sum / op.replayer.defaultRecords.RESMAN_Records.Count;
    }

    void onReplayEnded(MATBIISystem.MATBII_TASK task)
    {
        block = false;
    }
}
