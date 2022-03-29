using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TRACKTarget : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private Collider upBorder;
    [SerializeField] private Collider downBorder;
    [SerializeField] private Collider leftBorder;
    [SerializeField] private Collider rightBorder;
    [SerializeField] [Range(0.0f, 1.0f)] private float user_speed;
    private float theta = 0.0f;

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
    private float maxDistance = 0.0f;
    private double taskMeanDistance = 0.0;
    private int taskMeanDistanceIterator = 0;
    public double TaskMeanDistance() { return taskMeanDistance / (double) taskMeanDistanceIterator; }

    
    private Outline outline;
    private float outlineTimer = 0.0f;
    private bool outlined = false;

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
        
        //rb.AddForce(new Vector3(x,y,0).normalized * Time.deltaTime, ForceMode.Force);

        outline.enabled = true;
        outlineTimer = 1.0f;
        outlined = true;

        x *= Mathf.Abs(rightBorder.transform.position.x - leftBorder.transform.position.x) * Time.deltaTime * user_speed;
        if (transform.InverseTransformPoint(transform.position + new Vector3(x, 0, 0)).x < transform.InverseTransformPoint(leftBorder.transform.position).x
         || transform.InverseTransformPoint(transform.position + new Vector3(x, 0, 0)).x > transform.InverseTransformPoint(rightBorder.transform.position).x)
        { x=0; }

        y *= Mathf.Abs(   upBorder.transform.position.y - downBorder.transform.position.y) * Time.deltaTime * user_speed;
        if (transform.position.y + y >   upBorder.transform.position.y
         || transform.position.y + y <  downBorder.transform.position.y)
        { y=0; }
        
        transform.Translate(new Vector3(x, y, 0), Space.Self);
    }

    void Start()
    {
        origin = transform.localPosition;
        rb = GetComponent<Rigidbody>();
        mode = TrackingMode.auto;
        //PhotonNetwork.SerializationRate = 75;
        taskMeanDistance = 0.0;
        taskMeanDistanceIterator = 0;
        
        outline = GetComponent<Outline>();
        outline.enabled = false;
        outlined = false;
    }

    void Update()
    {
        if (outlineTimer > 0) outlineTimer -= Time.deltaTime;
        if (outlineTimer <= 0) {outline.enabled = false; outlined = false;}

        distance[dist_Iterator] = Distance(); dist_Iterator++;

        if (dist_Iterator >= distance.Length)
        {
            taskMeanDistance += MeanDistance();
            taskMeanDistanceIterator++;
            dist_Iterator = 0;
        }
        
        if (!photonView.IsMine) return;

        
        if (mode == TrackingMode.auto)
        {
            if (rb.velocity.magnitude >= 0.1f) rb.AddForce(Vector3.zero, ForceMode.VelocityChange);

            // small movements inside the inner zone
            theta = (theta + Time.deltaTime) % (2 * Mathf.PI);
            var x = Mathf.Cos(2 * theta) * 0.01f;
            var y = Mathf.Sin(3 * theta) * 0.01f;
            transform.localPosition = origin + new Vector3(x, y, 0);
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
                rb.velocity = new Vector3( velocityBeforePhysicsUpdate.x, -velocityBeforePhysicsUpdate.y, 0);
            }
            if (other.gameObject.name.Contains("Left") || other.gameObject.name.Contains("Right"))
            {
                rb.velocity = new Vector3( -velocityBeforePhysicsUpdate.x, velocityBeforePhysicsUpdate.y, 0);
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
        float distance = (transform.localPosition - origin).magnitude;
        if (distance > maxDistance) maxDistance = distance;
        return distance;
    }

    public float MaxDistance() { return maxDistance; }

    public float MeanDistance()
    {
        float sum = 0;
        for (int i = 0; i < distance.Length; i++)
        {
            sum += distance[i];
        }
        return sum / distance.Length;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(outlined);
            stream.SendNext(outlineTimer);
        }
        else
        {
            bool _outlined = (bool) stream.ReceiveNext();
            this.outlineTimer = (float) stream.ReceiveNext();
            if(_outlined && _outlined != outlined)
            {
                outlined = true;
                outline.enabled = true;
            }
        }
    }
}
