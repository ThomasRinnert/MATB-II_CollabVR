using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Globalization;

public class Replayer : MonoBehaviour
{
    public enum ReplaySource
    {
        DefaultRecords,
        CustomCSV
    }

    public enum ReplayState
    {
        Uninitialized,
        Loaded,
        Playing,
        Paused,
        Stopped
    }

    [Header("Object Bindings")]
    public Transform head;
    public Transform head_bone;
    public Transform handL;
    public Transform handL_bone;
    public Transform handR;
    public Transform handR_bone;
    public MvmtRecords defaultRecords;
    public Operator op;

    [Header("Attributes")]
    public ReplaySource source = ReplaySource.DefaultRecords;
    public MATBIISystem.MATBII_TASK TASK = MATBIISystem.MATBII_TASK.RESMAN;
    public int ID = 0;
    [Range(0.0f, 2.0f)] public float interpolationTime = 1.0f;
    [Range(0.0f, 20.0f)] public float duration = 10.0f;
    public ReplayState state = ReplayState.Uninitialized;
    
    // Internal attributes
    private MvmtRecord record;
    private string inputPath = string.Empty;
    private string[] lines = null;
    private CultureInfo ci;
    private int index;
    private int increment;
    private Coroutine replay;
    private System.DateTime startTime;
    private System.TimeSpan elapsedTime;
    
    public delegate void OnReplayEnded(MATBIISystem.MATBII_TASK task);
    OnReplayEnded onReplayEnded;

    public static float SYSMON_duration = 7.0f;
    public static float COMM_duration = 7.0f;
    public static float TRACK_duration = 13.0f;
    public static float RESMAN_duration = 13.0f;

    void Start()
    {
        if (op == null) op = GetComponent<Operator>();
    }

    public float getProgress()
    {
        return (record != null) ? (float)index / (float)record.Head_pos.Count : 0;
    }
    
    public void Play(MATBIISystem.MATBII_TASK _TASK, ReplaySource _source = ReplaySource.DefaultRecords, int _ID = -1, OnReplayEnded _callback = null)
    {
        if (_ID < 0)
        {
            switch (_TASK)
            {
                case MATBIISystem.MATBII_TASK.SYSMON:
                    _ID = UnityEngine.Random.Range(0, defaultRecords.SYSMON_Records.Count);
                    break;
                case MATBIISystem.MATBII_TASK.TRACK:
                    _ID = UnityEngine.Random.Range(0, defaultRecords.TRACK_Records.Count);
                    break;
                case MATBIISystem.MATBII_TASK.COMM:
                    _ID = UnityEngine.Random.Range(0, defaultRecords.COMM_Records.Count);
                    break;
                case MATBIISystem.MATBII_TASK.RESMAN:
                    _ID = UnityEngine.Random.Range(0, defaultRecords.RESMAN_Records.Count);
                    break;
                default:
                    Debug.LogError("REPLAYER: Undefined task");
                    return;
            }
        }

        source = _source;
        TASK = _TASK;
        ID = _ID;
        duration = _TASK.duration();
        onReplayEnded += _callback;

        Load();
        PlayPause();
    }

    [ContextMenu("Load")]
    public void Load()
    {
        if (state == ReplayState.Playing || state == ReplayState.Paused) { Stop(); }

        if (source == ReplaySource.DefaultRecords)
        {
            switch (TASK)
            {
                case MATBIISystem.MATBII_TASK.SYSMON:
                    if(ID >= defaultRecords.SYSMON_Records.Count)
                    {
                        Debug.LogError("REPLAYER: Wrong ID");
                    }
                    record = defaultRecords.SYSMON_Records[ID];
                    break;
                case MATBIISystem.MATBII_TASK.TRACK:
                    if(ID >= defaultRecords.TRACK_Records.Count)
                    {
                        Debug.LogError("REPLAYER: Wrong ID");
                    }
                    record = defaultRecords.TRACK_Records[ID];
                    break;
                case MATBIISystem.MATBII_TASK.COMM:
                    if(ID >= defaultRecords.COMM_Records.Count)
                    {
                        Debug.LogError("REPLAYER: Wrong ID");
                    }
                    record = defaultRecords.COMM_Records[ID];
                    break;
                case MATBIISystem.MATBII_TASK.RESMAN:
                    if(ID >= defaultRecords.RESMAN_Records.Count)
                    {
                        Debug.LogError("REPLAYER: Wrong ID");
                    }
                    record = defaultRecords.RESMAN_Records[ID];
                    break;
                default:
                    Debug.LogError("REPLAYER: Undefined task");
                    return;
            }
            index = 0;
        }
        else if(source == ReplaySource.CustomCSV)
        {
            /*
            string task;
            switch (TASK)
            {
                case MATBIISystem.MATBII_TASK.SYSMON:
                    task = "_SYSMON_";
                    break;
                case MATBIISystem.MATBII_TASK.TRACK:
                    task = "_TRACK_";
                    break;
                case MATBIISystem.MATBII_TASK.COMM:
                    task = "_COMM_";
                    break;
                case MATBIISystem.MATBII_TASK.RESMAN:
                    task = "_RESMAN_";
                    break;
                default:
                    Debug.LogError("REPLAYER: Undefined task");
                    return;
            }
            */
            inputPath =  Application.dataPath + "/MvmtRecords_" + ID.ToString() + ".csv";
            if(!File.Exists(inputPath))
            {
                Debug.LogError("REPLAYER: " + "MvmtRecords_" + ID.ToString() + ".csv" + " file does not exists");
                return;
            }
            lines = File.ReadAllLines(inputPath);
            ci = new CultureInfo("");
            index = 1;
        }

        state = ReplayState.Loaded;
    }

    [ContextMenu("Play | Pause")]
    public void PlayPause()
    {
        if(state == ReplayState.Loaded || state == ReplayState.Paused) state = ReplayState.Playing;
        else if(state == ReplayState.Playing) state = ReplayState.Paused;
        else return;
        
        if(state == ReplayState.Playing & source == ReplaySource.DefaultRecords) replay = StartCoroutine(DefaultReplaying());
        else if(state == ReplayState.Playing & source == ReplaySource.CustomCSV) replay = StartCoroutine(CSVReplaying());
    }

    [ContextMenu("Stop")]
    public void Stop()
    {
        state = ReplayState.Stopped;
        if (replay != null)
        {
            StopCoroutine(replay);
            replay = null;
        }
        if (onReplayEnded != null) onReplayEnded(TASK);
        onReplayEnded = null;
        index = 0;
    }

    private IEnumerator DefaultReplaying()
    {
        head.position = head_bone.position;
        head.rotation = head_bone.rotation;
        handL.position = handL_bone.position;
        handL.rotation = handL_bone.rotation;
        handR.position = handR_bone.position;
        handR.rotation = handR_bone.rotation;

        Vector3 head_pos = head.localPosition;
        Quaternion head_rot = head.localRotation;
        Vector3 handL_pos = handL.localPosition;
        Quaternion handL_rot = handL.localRotation;
        Vector3 handR_pos = handR.localPosition;
        Quaternion handR_rot = handR.localRotation;

        float r = 0;
        startTime = System.DateTime.UtcNow;
        elapsedTime = System.DateTime.UtcNow - startTime;
        
        // Interpolation from previous pose to initial replay pose
        while(state == ReplayState.Playing)
        {
            r = (float)(elapsedTime.TotalSeconds) / interpolationTime;

            head.localPosition = Vector3.Slerp(head_pos, record.Head_pos[0], r);
            head.localRotation = Quaternion.Slerp(head_rot, record.Head_rot[0], r);
            
            handL.localPosition = Vector3.Slerp(handL_pos, record.HandL_pos[0], r);
            handL.localRotation = Quaternion.Slerp(handL_rot, record.HandL_rot[0], r);
            
            handR.localPosition = Vector3.Slerp(handR_pos, record.HandR_pos[0], r);
            handR.localRotation = Quaternion.Slerp(handR_rot, record.HandR_rot[0], r);

            elapsedTime = System.DateTime.UtcNow - startTime;
            if((float)(elapsedTime.TotalSeconds) >= interpolationTime) {break;}
            
            yield return new WaitForSecondsRealtime(0.01f);
        }

        float timeStep = 0.0f; increment = 0;
        while (timeStep < 0.03)
        {
            increment++;
            elapsedTime = System.DateTime.UtcNow - startTime;
            timeStep = ((duration / op.speed) - (float)(elapsedTime.TotalSeconds)) / (record.Head_pos.Count / increment);
        }

        while(state == ReplayState.Playing)
        {
            elapsedTime = System.DateTime.UtcNow - startTime;

            head.localPosition = record.Head_pos[index];
            head.localRotation = record.Head_rot[index];
            
            handL.localPosition = record.HandL_pos[index];
            handL.localRotation = record.HandL_rot[index];
            
            handR.localPosition = record.HandR_pos[index];
            handR.localRotation = record.HandR_rot[index];

            index += increment;
            if(index >= record.Head_pos.Count - 1) { Stop(); }
            
            yield return new WaitForSecondsRealtime(timeStep - 0.01f);
        }
    }

    private IEnumerator CSVReplaying()
    {
        // TODO: Interpolation

        while(state == ReplayState.Playing)
        {
            string[] transforms = lines[index].Split(',');
            string[] headTr = transforms[0].Split(';');
            string[] handLTr = transforms[1].Split(';');
            string[] handRTr = transforms[2].Split(';');

            head.localPosition  = new Vector3(float.Parse(headTr[0], NumberStyles.Float, ci), float.Parse(headTr[1], NumberStyles.Float, ci), float.Parse(headTr[2], NumberStyles.Float, ci));
            head.rotation = new Quaternion(float.Parse(headTr[3], NumberStyles.Float, ci), float.Parse(headTr[4], NumberStyles.Float, ci), float.Parse(headTr[5], NumberStyles.Float, ci), float.Parse(headTr[6], NumberStyles.Float, ci));
            
            handL.localPosition = new Vector3(float.Parse(handLTr[0], NumberStyles.Float, ci), float.Parse(handLTr[1], NumberStyles.Float, ci), float.Parse(handLTr[2], NumberStyles.Float, ci));
            handL.rotation = new Quaternion(float.Parse(handLTr[3], NumberStyles.Float, ci), float.Parse(handLTr[4], NumberStyles.Float, ci), float.Parse(handLTr[5], NumberStyles.Float, ci), float.Parse(handLTr[6], NumberStyles.Float, ci));
            
            handR.localPosition = new Vector3(float.Parse(handRTr[0], NumberStyles.Float, ci), float.Parse(handRTr[1], NumberStyles.Float, ci), float.Parse(handRTr[2], NumberStyles.Float, ci));
            handR.rotation = new Quaternion(float.Parse(handRTr[3], NumberStyles.Float, ci), float.Parse(handRTr[4], NumberStyles.Float, ci), float.Parse(handRTr[5], NumberStyles.Float, ci), float.Parse(handRTr[6], NumberStyles.Float, ci));

            index++;
            if(index >= lines.Length - 1) Stop();
            
            yield return new WaitForSecondsRealtime(0.01f);
        }
        Stop();
    }
}

public static class MATBII_TASKExtension
{
    public static float duration(this MATBIISystem.MATBII_TASK task)
    {
        float duration = 0.0f;
        if (task == MATBIISystem.MATBII_TASK.COMM) duration = Replayer.COMM_duration;
        else if (task == MATBIISystem.MATBII_TASK.TRACK) duration = Replayer.TRACK_duration;
        else if (task == MATBIISystem.MATBII_TASK.RESMAN) duration = Replayer.RESMAN_duration;
        else if (task == MATBIISystem.MATBII_TASK.SYSMON) duration = Replayer.SYSMON_duration;
        return duration;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Replayer)), CanEditMultipleObjects]
public class ReplayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Replayer script = (Replayer)target;

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Load"))
        {
            script.Load();
        }
        if(GUILayout.Button("Play / Pause"))
        {
            script.PlayPause();
        }
        if(GUILayout.Button("Stop"))
        {
            script.Stop();
        }
        GUILayout.EndHorizontal();
    }
}
#endif