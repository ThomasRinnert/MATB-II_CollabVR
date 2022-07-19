using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Recorder : MonoBehaviour
{
    [Header("Object Bindings")]
    [SerializeField] public PlayerManagerVR managerVR;
    [SerializeField] public Transform root;
    [SerializeField] public Transform head;
    [SerializeField] public Transform handL;
    [SerializeField] public Transform handR;

    [Header("Attributes")]
    [SerializeField] public uint ID = 0;
    private string outputPath;
    private bool recording = false;
    private Coroutine record;
    private System.Globalization.CultureInfo sf = System.Globalization.CultureInfo.InvariantCulture;

    [ContextMenu("Bind")]
    bool Binding()
    {
        managerVR = GetComponent<PlayerManagerVR>();
        if(managerVR == null)
        {
            Debug.LogError("No VR PlayerManager attached to the recorder");
            return false;
        }managerVR = GetComponent<PlayerManagerVR>();
        root = managerVR.transform;
        if(root == null)
        {
            Debug.LogError("No root Transform attached to the recorder");
            return false;
        }
        head = managerVR.HeadTarget;
        if(head == null)
        {
            Debug.LogError("No head target attached to the player manager");
            return false;
        }
        handL = managerVR.HandLTarget;
        if(handL == null)
        {
            Debug.LogError("No left hand target attached to the player manager");
            return false;
        }
        handR = managerVR.HandRTarget;
        if(handR == null)
        {
            Debug.LogError("No right hand target attached to the player manager");
            return false;
        }

        return true;
    }

    [ContextMenu("Start recording")]
    void StartRecording()
    {
        outputPath = Application.dataPath + "/MvmtRecords_" + ID + ".csv";
        string log_header = "HEAD,HANDL,HANDR\n";
        File.WriteAllText(outputPath, log_header);

        recording = true;

        record = StartCoroutine(Recording());
    }

    [ContextMenu("Stop recording")]
    void StopRecording()
    {
        recording = false;
    }

    private IEnumerator Recording()
    {
        while(recording)
        {
            Vector3 H     = head.position - root.position;
            Quaternion Hr = head.rotation;
            Vector3 L     = handL.position - root.position;
            Quaternion Lr = handL.rotation;
            Vector3 R     = handR.position - root.position;
            Quaternion Rr = handR.rotation;
            
            string text = 
            H.x.ToString(sf) + ';' + H.y.ToString(sf) + ';' + H.z.ToString(sf) + ';' + Hr.x.ToString(sf) + ';' +Hr.y.ToString(sf) + ';' +Hr.z.ToString(sf) + ';' +Hr.w.ToString(sf) + ',' +
            L.x.ToString(sf) + ';' + L.y.ToString(sf) + ';' + L.z.ToString(sf) + ';' + Lr.x.ToString(sf) + ';' +Lr.y.ToString(sf) + ';' +Lr.z.ToString(sf) + ';' +Lr.w.ToString(sf) + ',' +
            R.x.ToString(sf) + ';' + R.y.ToString(sf) + ';' + R.z.ToString(sf) + ';' + Rr.x.ToString(sf) + ';' +Rr.y.ToString(sf) + ';' +Rr.z.ToString(sf) + ';' +Rr.w.ToString(sf) + '\n';
            File.AppendAllText(outputPath, text);

            yield return new WaitForSecondsRealtime(0.01f);
        }
    }

    void Start()
    {
        if(head == null || handL == null || handR == null) { Binding(); }
    }
}
