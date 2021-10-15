using UnityEngine;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public class SYSMONSwitch : Interactive
{
    [SerializeField]
    public bool normallyON = true;

    private bool pressing = false;
    public bool isPressed() { return pressing; }

    private Transform author;
    private Outline outline;

    void Start()
    {
        Material m = GetComponent<Renderer>().material;
        m.color = normallyON ? Color.green : Color.gray;
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand" || other.tag == "Cursor")
        {
            pressing = true;
            author = other.transform;
            outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand" || other.tag == "Cursor")
        {
            pressing = false;
            outline.enabled = false;
        }

    }

    public void Activate()
    {
        Material m = GetComponent<Renderer>().material;
        m.color = normallyON ? Color.gray : Color.red;
    }

    public void Restore()
    {
        Material m = GetComponent<Renderer>().material;
        m.color = normallyON ? Color.green : Color.gray;
    }

    override public void Click()
    {
        if(!pressing) return;
        object[] content = new object[] {author.GetComponentInParent<PhotonView>().Owner.NickName, normallyON};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.SYSMON_SwitchPressed, content, raiseEventOptions, SendOptions.SendReliable);
        pressing = false;
    }
    override public void Release() {}
}
