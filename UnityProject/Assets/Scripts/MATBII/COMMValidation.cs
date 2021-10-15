using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class COMMValidation : VRUIButton
{
    private Photon.Realtime.Player author = null;

    new protected void Start()
    {
        base.Start();
    }
    
    new public void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        author = other.GetComponentInParent<Photon.Pun.PhotonView>().Owner;

    }

    new public void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        author = other.GetComponentInParent<Photon.Pun.PhotonView>().Owner;

    }

    public void Validate()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.COMM_Validation, new object[] {author.NickName}, raiseEventOptions, SendOptions.SendReliable);
    }
}