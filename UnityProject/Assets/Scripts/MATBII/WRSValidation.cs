
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class WRSValidation : VRUIButton
{
    [SerializeField] private WRSSlider mental;
    [SerializeField] private WRSSlider physical;

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
        var content = new object[] {author.NickName, mental.value, physical.value};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.WRS_Validation, content, raiseEventOptions, ExitGames.Client.Photon.SendOptions.SendReliable);
    }
}
