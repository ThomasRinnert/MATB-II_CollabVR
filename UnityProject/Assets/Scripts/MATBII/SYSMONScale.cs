using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class SYSMONScale : MonoBehaviour, Interactive
{
    [SerializeField] private int index = 0;
    [SerializeField] private Collider lowerBorder;
    [SerializeField] private Collider upperBorder;
    private Outline outline;
    private Transform author;

    private bool outOfBounds = false;
    public bool isOutOfBounds() { return outOfBounds; }
    
    private bool pressing = false;
    public bool isPressed() { return pressing; }

    private Vector3 origin;
    public void ResetPos() { transform.localPosition = origin; speed = 0.0f; }
    
    private float speed = 0.0f;
    public void Move(float timeLimit, bool upward)
    {
        float y = upward ? upperBorder.transform.position.y : lowerBorder.transform.position.y;
        this.speed = (2 * (y - transform.position.y)) / timeLimit;
    }
    
    void Start()
    {
        origin = transform.localPosition;
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    void Update()
    {
        if (speed != 0.0f) transform.Translate(Time.deltaTime * speed * transform.up);
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

    public void Click()
    {
        if(!pressing) return;

        object[] content = new object[] {author.GetComponentInParent<PhotonView>().Owner.NickName, index};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(MATBIISystem.SYSMON_ScalePressed, content, raiseEventOptions, SendOptions.SendReliable);
        pressing = false;
    }
    public void Release() {}
}
