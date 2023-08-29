using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OperatorStatusPanel : MonoBehaviour
{
    bool Literal  = false; public bool getLiteral()  { return Literal;  }
    bool Symbolic = false; public bool getSymbolic() { return Symbolic; }
    bool stress = false; public bool getStress() { return stress; }

    public void ToggleLiteral() { ShowLiteral(!Literal); }
    public void ShowLiteral(bool b = true)
    {
        Literal = b;
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            if (child.tag == "Literal") child.gameObject.SetActive(b);
        }

        //GetComponentInChildren<TaskHistory>(true).gameObject.SetActive((Symbolic || Literal));
        GetComponentInChildren<TaskQueueDisplay>(true).gameObject.SetActive((Symbolic || Literal));
    }

    public void ToggleSymbolic() { ShowSymbolic(!Symbolic); }
    public void ShowSymbolic(bool b = true)
    {
        Symbolic = b;
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (var child in children)
        {
            if (child.tag == "Symbolic") child.gameObject.SetActive(b);
        }

        //GetComponentInChildren<TaskHistory>(true).gameObject.SetActive((Symbolic || Literal));
        GetComponentInChildren<TaskQueueDisplay>(true).gameObject.SetActive((Symbolic || Literal));
    }

    public void ToggleStress() { ShowStress(!stress); }
    public void ShowStress(bool b)
    {
        stress = b;
        StressOverTime stressOverTime = GetComponentInChildren<StressOverTime>(true);
        stressOverTime.gameObject.SetActive(b);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(OperatorStatusPanel)), CanEditMultipleObjects]
public class OperatorStatusPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OperatorStatusPanel script = (OperatorStatusPanel)target;

        GUILayout.BeginHorizontal();
        //GUILayout.Label("Total task count: " + script.getTaskCount().ToString());
        if(GUILayout.Button((script.getLiteral() ? "Hide" : "Show") + " Literal"))
        {
            script.ToggleLiteral();
        }
        if(GUILayout.Button((script.getSymbolic() ? "Hide" : "Show") + " Symbolic"))
        {
            script.ToggleSymbolic();
        }
        GUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
#endif