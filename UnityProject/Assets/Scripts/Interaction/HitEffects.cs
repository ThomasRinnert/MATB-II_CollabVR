using UnityEngine;

public class HitEffects : MonoBehaviour
{
    //public Collider targetCollider;

    public GameObject spawnObjectOnCollision;

    public bool destroyOnTargetCollision = true;

    [Range(0,1)]
    public float delay = 0.01f;
    
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        //if (collider == targetCollider)
        if (collider.GetComponent<TaskReceiver>() != null)
        {
            GameObject spawned = GameObject.Instantiate(spawnObjectOnCollision);
            spawned.transform.position = this.transform.position;
            Rigidbody[] rbs = spawned.transform.GetComponentsInChildren<Rigidbody>();
            foreach (var r in rbs) { r.AddForce(rb.velocity, ForceMode.VelocityChange); }
            
            Invoke("delayedCollision", delay);
        }
    }

    private void delayedCollision()
    {
        if (destroyOnTargetCollision) {Destroy(this.gameObject);}
    }
}
