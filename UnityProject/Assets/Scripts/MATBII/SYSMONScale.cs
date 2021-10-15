using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SYSMONScale : Interactive, IPunObservable 
{
    [SerializeField] private int index = 0;
    [SerializeField] private Collider upperBorder;
    [SerializeField] private Collider lowerBorder;
    private Outline outline;
    private Transform author;

    private bool outOfBounds = false;
    public bool isOutOfBounds() { return outOfBounds; }
    
    private bool pressing = false;
    public bool isPressed() { return pressing; }

    private Vector3 origin;
    public void ResetPos() { transform.localPosition = origin; speed = 0.0f; theta = 0.0f;}
    [SerializeField] [Range(-Mathf.PI, Mathf.PI)] private float theta = 0.0f;
    private float speed = 0.0f;
    private float rspeed = 0.0f;
    public void Move(float timeLimit, bool upward)
    {
        ResetPos();
        float y = upward ? upperBorder.transform.position.y : lowerBorder.transform.position.y;
        this.speed = (2 * (y - transform.position.y)) / timeLimit;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(theta);
        }
        else
        {
            theta = (float) stream.ReceiveNext();
        }
    }
    
    void Start()
    {
        origin = transform.localPosition;
        outline = GetComponent<Outline>();
        outline.enabled = false;
        rspeed = Random.Range(0.0f, 2.0f);
    }

    void Update()
    {
        //if (!photonView.IsMine) return;
        if (speed != 0.0f) transform.Translate(Time.deltaTime * speed * transform.up);
        else if (!MATBIISystem.Instance.isSYSMON_Scales_active(index))
        {
            // small movements inside the inner zone
            theta = (theta + Time.deltaTime) % (2 * Mathf.PI);
            var y = Mathf.Sin(theta * rspeed) * 0.005f;
            //transform.Translate(new Vector3(x, y, 0), Space.Self);
            transform.localPosition = origin + new Vector3(0, y, 0);
        }
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Bounds")
        {
            outOfBounds = true;
            speed = 0.0f;
        }
        if (other.tag == "LeftHand" || other.tag == "RightHand" || other.tag == "Cursor")
        {
            pressing = true;
            author = other.transform;
            outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Bounds")
        {
            outOfBounds = false;
        }
        if (other.tag == "LeftHand" || other.tag == "RightHand" || other.tag == "Cursor")
        {
            pressing = false;
            outline.enabled = false;
        }

    }

    override public void Click()
    {
        if(!pressing) return;

        object[] content = new object[] {author.GetComponentInParent<PhotonView>().Owner.NickName, index};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.SYSMON_ScalePressed, content, raiseEventOptions, SendOptions.SendReliable);
        pressing = false;
        theta = 0;
    }
    override public void Release() {}

    
}
