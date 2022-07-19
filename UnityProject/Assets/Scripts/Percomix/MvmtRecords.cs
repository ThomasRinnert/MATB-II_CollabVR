using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[Serializable]
public class MvmtRecord
{
    public string name;
    public string path;
    //public MATBIISystem.MATBII_TASK task;
    public List<Vector3>    Head_pos  = new List<Vector3>();
    public List<Quaternion> Head_rot  = new List<Quaternion>();
    public List<Vector3>    HandL_pos = new List<Vector3>();
    public List<Quaternion> HandL_rot = new List<Quaternion>();
    public List<Vector3>    HandR_pos = new List<Vector3>();
    public List<Quaternion> HandR_rot = new List<Quaternion>();
}

[CreateAssetMenu(fileName = "MvmtRecords", menuName = "ScriptableObjects/MvmtRecords", order = 2)]
public class MvmtRecords : ScriptableObject
{
    [SerializeField] [Tooltip("Path to the folder conatining movements records csv files")]
    private string path = "/MovementsRecords/";
    [ContextMenu("Default Path")] public void SetDefaultPath() { path = Application.dataPath + "/Scripts/Scenarii/MovementsRecords/"; }
    
    public List<MvmtRecord> SYSMON_Records = new List<MvmtRecord>();
    public List<MvmtRecord> COMM_Records = new List<MvmtRecord>();
    public List<MvmtRecord> TRACK_Records = new List<MvmtRecord>();
    public List<MvmtRecord> RESMAN_Records = new List<MvmtRecord>();

    private string[] lines;
    private CultureInfo ci;

    [ContextMenu("Init Lists")]
    public void InitLists()
    {
        SYSMON_Records = new List<MvmtRecord>();
        COMM_Records = new List<MvmtRecord>();
        TRACK_Records = new List<MvmtRecord>();
        RESMAN_Records = new List<MvmtRecord>();
    }

    [ContextMenu("Extract Records")]
    public void ExtractRecords()
    {
        if(SYSMON_Records == null) SYSMON_Records = new List<MvmtRecord>();
        if(COMM_Records == null) COMM_Records = new List<MvmtRecord>();
        if(TRACK_Records == null) TRACK_Records = new List<MvmtRecord>();
        if(RESMAN_Records == null) RESMAN_Records = new List<MvmtRecord>();

        string[] files = Directory.GetFiles(path);
        ci = new CultureInfo("");
        foreach(string file in files)
        {
            // Filter the files
            if(file.Contains(".asset") || file.Contains(".meta") || file.Contains("raw")) continue;
            List<MvmtRecord> dest;
            if(file.Contains("SYSMON")) dest = SYSMON_Records;
            else if(file.Contains("COMM")) dest = COMM_Records;
            else if(file.Contains("TRACK")) dest = TRACK_Records;
            else if(file.Contains("RESMAN")) dest = RESMAN_Records;
            else continue;
            // Debug.Log(file);

            // Init the record object
            MvmtRecord record = new MvmtRecord();
            record.path = file;
            var name = file.Split('/');
            record.name = name[name.Length-1];

            //Converts CSV lines to Transforms
            lines = File.ReadAllLines(file);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] transforms = lines[i].Split(',');
                string[] headTr = transforms[0].Split(';');
                string[] handLTr = transforms[1].Split(';');
                string[] handRTr = transforms[2].Split(';');

                record.Head_pos.Add(new Vector3(float.Parse(headTr[0], NumberStyles.Float, ci), float.Parse(headTr[1], NumberStyles.Float, ci), float.Parse(headTr[2], NumberStyles.Float, ci)));
                record.Head_rot.Add(new Quaternion(float.Parse(headTr[3], NumberStyles.Float, ci), float.Parse(headTr[4], NumberStyles.Float, ci), float.Parse(headTr[5], NumberStyles.Float, ci), float.Parse(headTr[6], NumberStyles.Float, ci)));
                
                record.HandL_pos.Add(new Vector3(float.Parse(handLTr[0], NumberStyles.Float, ci), float.Parse(handLTr[1], NumberStyles.Float, ci), float.Parse(handLTr[2], NumberStyles.Float, ci)));
                record.HandL_rot.Add(new Quaternion(float.Parse(handLTr[3], NumberStyles.Float, ci), float.Parse(handLTr[4], NumberStyles.Float, ci), float.Parse(handLTr[5], NumberStyles.Float, ci), float.Parse(handLTr[6], NumberStyles.Float, ci)));
                
                record.HandR_pos.Add(new Vector3(float.Parse(handRTr[0], NumberStyles.Float, ci), float.Parse(handRTr[1], NumberStyles.Float, ci), float.Parse(handRTr[2], NumberStyles.Float, ci)));
                record.HandR_rot.Add(new Quaternion(float.Parse(handRTr[3], NumberStyles.Float, ci), float.Parse(handRTr[4], NumberStyles.Float, ci), float.Parse(handRTr[5], NumberStyles.Float, ci), float.Parse(handRTr[6], NumberStyles.Float, ci)));
            }
            dest.Add(record);
        }
    }
}
