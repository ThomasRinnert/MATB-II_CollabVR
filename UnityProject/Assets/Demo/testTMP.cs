using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class testTMP : MonoBehaviour
{
    [SerializeField] GameObject obj;
    GameObject HeadYaw; float w = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(HeadYaw != null)
        {
            w += Time.deltaTime;
            HeadYaw.transform.rotation = Quaternion.Euler(0,Mathf.Sin(w)*45,0);
        }
        else if (w != 0.0f)
        {
            w = 0.0f;
        }
    }

    public void Spawn()
    {
        Instantiate(obj, new Vector3(Random.Range(-0.2f, 0.2f),2,Random.Range(-0.2f, 0.2f)), Quaternion.identity);
    }
    
    public void SayNoToDrugs()
    {
        if (HeadYaw == null)
        {
            HeadYaw = GameObject.Find("HeadYaw");
        }
        else
        {
            HeadYaw.transform.rotation = Quaternion.identity;
            w = 0.0f;
            HeadYaw = null;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(testTMP)), CanEditMultipleObjects]
public class testTMPEditor : Editor
{
    public override void OnInspectorGUI()
    {
        testTMP script = (testTMP)target;

        if(GUILayout.Button("Spawn"))
        {
            script.Spawn();
        }

        if(GUILayout.Button("SayNoToDrugs"))
        {
            script.SayNoToDrugs();
        }

        DrawDefaultInspector();
    }
}
#endif
