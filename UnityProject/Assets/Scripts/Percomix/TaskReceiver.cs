using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskReceiver : MonoBehaviour
{
    public Operator op;
    public TaskSpawning spawner;

    [ContextMenu("Bind")]
    public void Bind()
    {
        op = transform.parent.parent.GetComponentInChildren<Operator>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (op == null && spawner == null) return;
        TaskArtifact artifact = collider.gameObject.GetComponentInParent<TaskArtifact>();
        if (artifact == null) return;
        
        // If linked to an operator
        if (op != null) { op.GiveTask(artifact.task); ExperimentControlsMATBIITeam.Instance.LogTaskAssigned(op, artifact.task); }
        
        // If it's a receiver for tasks that missed the actual receivers
        else if (spawner != null) {spawner.SpawnTask(artifact.task); ExperimentControlsMATBIITeam.Instance.LogTaskMissed(artifact.task); }
    }
}
