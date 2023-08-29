using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class testTMP : MonoBehaviour
{
    [SerializeField] GameObject obj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Function1()
    {
        Renderer r = GetComponent<Renderer>();
        r.material.SetColor("_EmissionColor", Color.green);
    }
    
    public void Function2()
    {
        Renderer r = GetComponent<Renderer>();
        r.material.SetColor("_EmissionColor", Color.red);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(testTMP)), CanEditMultipleObjects]
public class testTMPEditor : Editor
{
    public override void OnInspectorGUI()
    {
        testTMP script = (testTMP)target;

        if(GUILayout.Button("Function1"))
        {
            script.Function1();
        }

        if(GUILayout.Button("Function2"))
        {
            script.Function2();
        }

        DrawDefaultInspector();
    }
}
#endif
