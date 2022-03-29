using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TASKPlanningElement
{
    public float startTime;
    //public TASKPlanningParameters parameters;
}
[Serializable]
public class TASKPlanningParameters {}

[Serializable]
public class SYSMON_Task : TASKPlanningElement
{
    public SYSMON_Parameters parameters;
    SYSMON_Task() { parameters = new SYSMON_Parameters(); }
}
[Serializable]
public class SYSMON_Parameters : TASKPlanningParameters
{
    public bool NormallyON = true;
    public bool NormallyOFF = true;
    public bool scale1 = true;
    public bool scale2 = true;
    public bool scale3 = true;
    public bool scale4 = true;
    
    public void randomize()
    {
        NormallyON  = UnityEngine.Random.Range(float.MinValue, float.MaxValue) > 0.0f; 
        NormallyOFF = UnityEngine.Random.Range(float.MinValue, float.MaxValue) > 0.0f; 
        scale1      = UnityEngine.Random.Range(float.MinValue, float.MaxValue) > 0.0f; 
        scale2      = UnityEngine.Random.Range(float.MinValue, float.MaxValue) > 0.0f; 
        scale3      = UnityEngine.Random.Range(float.MinValue, float.MaxValue) > 0.0f; 
        scale4      = UnityEngine.Random.Range(float.MinValue, float.MaxValue) > 0.0f; 
    }
}

[Serializable]
public class COMM_Task : TASKPlanningElement
{
    public COMM_Parameters parameters;
    COMM_Task() { parameters = new COMM_Parameters(); }
}
[Serializable]
public class COMM_Parameters : TASKPlanningParameters
{
}

[Serializable]
public class TRACK_Task : TASKPlanningElement
{
    public TRACK_Parameters parameters;
    TRACK_Task() { parameters = new TRACK_Parameters(); }
}
[Serializable]
public class TRACK_Parameters : TASKPlanningParameters
{
}

[Serializable]
public class RESMAN_Task : TASKPlanningElement
{
    public RESMAN_Parameters parameters;
    RESMAN_Task() { parameters = new RESMAN_Parameters(); }
}
[Serializable]
public class RESMAN_Parameters : TASKPlanningParameters
{
}

[Serializable]
public class WrkLd_Task : TASKPlanningElement
{
    public WrkLd_Parameters parameters;
    WrkLd_Task() { parameters = new WrkLd_Parameters(); }
}
[Serializable]
public class WrkLd_Parameters : TASKPlanningParameters
{
}

[CreateAssetMenu(fileName = "Schedule", menuName = "ScriptableObjects/TASKPlanning", order = 1)]
public class TASKPlanning : ScriptableObject
{
    [Range(0.0f, 600.0f)]
    public double duration = 60;
    public List<SYSMON_Task> SYSMON_Tasks = new List<SYSMON_Task>();
    public List<COMM_Task> COMM_Tasks = new List<COMM_Task>();
    public List<TRACK_Task> TRACK_Tasks = new List<TRACK_Task>();
    public List<RESMAN_Task> RESMAN_Tasks = new List<RESMAN_Task>();
    public List<WrkLd_Task> WrkLd_Tasks = new List<WrkLd_Task>();
    
    [ContextMenu("SORT")]
    public void SORT()
    {
        SYSMON_Tasks.Sort((t1,t2)=>t1.startTime.CompareTo(t2.startTime));
        COMM_Tasks.Sort((t1,t2)=>t1.startTime.CompareTo(t2.startTime));
        TRACK_Tasks.Sort((t1,t2)=>t1.startTime.CompareTo(t2.startTime));
        RESMAN_Tasks.Sort((t1,t2)=>t1.startTime.CompareTo(t2.startTime));
        WrkLd_Tasks.Sort((t1,t2)=>t1.startTime.CompareTo(t2.startTime));
    }
    
    [ContextMenu("Randomize SYSMON params")]
    public void RandomizeSYSMONparams()
    {
        foreach (var task in SYSMON_Tasks) { task.parameters.randomize(); }
    }
    
    [ContextMenu("DEBUG")]
    public void DEBUG()
    {
        Debug.Log("DEBUG");
    }
}
/*
// See https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
[CustomPropertyDrawer(typeof(TASKPlanningElement))]
public class TASKPlanningElementUIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var startTime = new Rect(position.x, position.y, (position.width / 2) - 5, position.height);
        var task = new Rect(position.x + (position.width / 2), position.y, (position.width / 2), position.height);

        EditorGUI.PropertyField(startTime, property.FindPropertyRelative("startTime"), GUIContent.none);
        EditorGUI.PropertyField(task, property.FindPropertyRelative("task"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
*/
/*
[CustomPropertyDrawer(typeof(SYSMON_Parameters))]
public class SYSMON_TaskUIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var boolWidth = .8f * position.height;
        var lightWidth = (position.width - 6.0f * boolWidth) / 4.0f;
        var scaleWidth = (position.width - 6.0f * boolWidth) / 8.0f;

        var NormallyON =     new Rect(position.x,                                                   position.y, boolWidth,  position.height);
        EditorGUI.LabelField(new Rect(position.x +     boolWidth,                                   position.y, lightWidth, position.height), "L_ON");
        var NormallyOFF =    new Rect(position.x +     boolWidth +     lightWidth,                  position.y, boolWidth,  position.height);
        EditorGUI.LabelField(new Rect(position.x + 2 * boolWidth +     lightWidth,                  position.y, lightWidth, position.height), "L_OFF"); 
        var scale1 =         new Rect(position.x + 2 * boolWidth + 2 * lightWidth,                  position.y, boolWidth,  position.height);
        EditorGUI.LabelField(new Rect(position.x + 3 * boolWidth + 2 * lightWidth,                  position.y, scaleWidth, position.height), "S_0");
        var scale2 =         new Rect(position.x + 3 * boolWidth + 2 * lightWidth +     scaleWidth, position.y, boolWidth,  position.height);
        EditorGUI.LabelField(new Rect(position.x + 4 * boolWidth + 2 * lightWidth +     scaleWidth, position.y, scaleWidth, position.height), "S_1");
        var scale3 =         new Rect(position.x + 4 * boolWidth + 2 * lightWidth + 2 * scaleWidth, position.y, boolWidth,  position.height);
        EditorGUI.LabelField(new Rect(position.x + 5 * boolWidth + 2 * lightWidth + 2 * scaleWidth, position.y, scaleWidth, position.height), "S_2");
        var scale4 =         new Rect(position.x + 5 * boolWidth + 2 * lightWidth + 3 * scaleWidth, position.y, boolWidth,  position.height);
        EditorGUI.LabelField(new Rect(position.x + 6 * boolWidth + 2 * lightWidth + 3 * scaleWidth, position.y, scaleWidth, position.height), "S_3");

        EditorGUI.PropertyField(NormallyON, property.FindPropertyRelative("NormallyON"), GUIContent.none);
        EditorGUI.PropertyField(NormallyOFF, property.FindPropertyRelative("NormallyOFF"), GUIContent.none);
        EditorGUI.PropertyField(scale1, property.FindPropertyRelative("scale1"), GUIContent.none);
        EditorGUI.PropertyField(scale2, property.FindPropertyRelative("scale2"), GUIContent.none);
        EditorGUI.PropertyField(scale3, property.FindPropertyRelative("scale3"), GUIContent.none);
        EditorGUI.PropertyField(scale4, property.FindPropertyRelative("scale4"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
*/
/*
[CustomPropertyDrawer(typeof(TASKPlanningElement))]
public class TASKPlanningElementUIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var startTime = new Rect(position.x, position.y, (position.width / 2) - 5, position.height);
        var task = new Rect(position.x + (position.width / 2), position.y, (position.width / 2), position.height);

        EditorGUI.PropertyField(startTime, property.FindPropertyRelative("startTime"), GUIContent.none);
        EditorGUI.PropertyField(task, property.FindPropertyRelative("task"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
[CustomPropertyDrawer(typeof(TASKPlanningElement))]
public class TASKPlanningElementUIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var startTime = new Rect(position.x, position.y, (position.width / 2) - 5, position.height);
        var task = new Rect(position.x + (position.width / 2), position.y, (position.width / 2), position.height);

        EditorGUI.PropertyField(startTime, property.FindPropertyRelative("startTime"), GUIContent.none);
        EditorGUI.PropertyField(task, property.FindPropertyRelative("task"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
[CustomPropertyDrawer(typeof(TASKPlanningElement))]
public class TASKPlanningElementUIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var startTime = new Rect(position.x, position.y, (position.width / 2) - 5, position.height);
        var task = new Rect(position.x + (position.width / 2), position.y, (position.width / 2), position.height);

        EditorGUI.PropertyField(startTime, property.FindPropertyRelative("startTime"), GUIContent.none);
        EditorGUI.PropertyField(task, property.FindPropertyRelative("task"), GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
*/