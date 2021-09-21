using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TRACKTarget : MonoBehaviourPun
{
    private enum TrackingMode
    {
        auto,
        wander
    }
    private TrackingMode mode = TrackingMode.auto;

    private bool innerZone = false;
    public bool inInnerZone() { return innerZone; }

    private Vector3 origin;
    public void ResetPos() { transform.localPosition = origin; rb.velocity = Vector3.zero; }

    [SerializeField]
    private float[] distance = new float[100];
    private int dist_Iterator = 0;

    private Rigidbody rb;
    public void Move(Vector3 position, Vector3 direction, float speed)
    {
        if (!photonView.IsMine) return;
        mode = TrackingMode.wander;
        transform.position = position;
        rb.AddForce(direction.normalized * speed * Time.deltaTime, ForceMode.VelocityChange);
    }

    public void Move(float x, float y)
    {
        if (mode == TrackingMode.auto || !photonView.IsMine) return;
        rb.AddForce(new Vector3(x,y,0).normalized * Time.deltaTime * 0.7f, ForceMode.VelocityChange);
        //transform.Translate(new Vector3( x * Time.deltaTime, y * Time.deltaTime, 0), Space.World);
    }

    void Start()
    {
        origin = transform.localPosition;
        rb = GetComponent<Rigidbody>();
        mode = TrackingMode.auto;
        //PhotonNetwork.SerializationRate = 75;
    }

    void Update()
    {
        distance[dist_Iterator] = Distance();
        dist_Iterator = (dist_Iterator + 1) % distance.Length;
        
        if (!photonView.IsMine) return;

        if(innerZone)
        {
            if ((origin - transform.localPosition).magnitude >= 0.01f)
            {
                rb.AddForce(origin - transform.localPosition * Time.deltaTime, ForceMode.Acceleration);
            }
            else
            {
                // small movements inside the inner zone
                if (rb.velocity.magnitude >= 0.1f) rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
            }
        }
        
        if (mode == TrackingMode.auto)
        {
            // small movements inside the inner zone
            if (rb.velocity.magnitude >= 0.1f) rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
        }
    }

    private Vector3 velocityBeforePhysicsUpdate;
    void FixedUpdate()
    {
        velocityBeforePhysicsUpdate = rb.velocity;
    }

    void OnCollisionEnter(Collision other)
    {
        if (!photonView.IsMine) return;

        if (other.gameObject.tag == "Bounds")
        {
            if (other.gameObject.name.Contains("Up") || other.gameObject.name.Contains("Down"))
            {
                rb.velocity = new Vector3( velocityBeforePhysicsUpdate.x, -velocityBeforePhysicsUpdate.y, 0) * 0.7f;
            }
            if (other.gameObject.name.Contains("Left") || other.gameObject.name.Contains("Right"))
            {
                rb.velocity = new Vector3( -velocityBeforePhysicsUpdate.x, velocityBeforePhysicsUpdate.y, 0) * 0.7f;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bounds")
        {
            innerZone = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Bounds")
        {
            innerZone = false;
        }
    }

    public void Activate()
    {
        mode = TrackingMode.wander;
    }

    public void Restore()
    {
        ResetPos();
        mode = TrackingMode.auto;
    }

    public float Distance()
    {
        return (transform.localPosition - origin).magnitude;
    }

    public float MeanDistance()
    {
        float sum = 0;
        for (int i = 0; i < distance.Length; i++)
        {
            sum += distance[i];
        }
        return sum / distance.Length;
    }
}
