using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskReceiver : MonoBehaviour
{
    public Operator op;

    [ContextMenu("Bind")]
    public void Bind()
    {
        op = transform.parent.parent.GetComponentInChildren<Operator>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (op == null) return;
        TaskArtifact artifact = collider.gameObject.GetComponentInParent<TaskArtifact>();
        if (artifact == null) return;
        
        op.GiveTask(artifact.task);
    }
}
