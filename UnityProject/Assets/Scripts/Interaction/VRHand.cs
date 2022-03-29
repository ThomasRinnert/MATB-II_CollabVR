using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Valve.VR;

public class VRHand : MonoBehaviourPun
{
    public enum Handedness { Left, Right }
    public Handedness hand; 
    public SteamVR_Action_Boolean click1 = null;
    public SteamVR_Action_Boolean click2 = null;

    public SteamVR_Action_Vector2 direction;

    private string author;
    private bool caught = false;
    [SerializeField] private Interactive target = null;

    void Start()
    {
        var pView = GetComponentInParent<PhotonView>();
        if (pView.Owner != null) author = pView.Owner.NickName;
        if (photonView.IsMine  || ! PhotonNetwork.IsConnected)
        {
            SteamVR_Input_Sources source = hand == Handedness.Left ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            
            click1.AddOnStateDownListener(Catch, source);
            click1.AddOnStateUpListener(Release, source);

            click2.AddOnStateDownListener(Catch, source);
            click2.AddOnStateUpListener(Release, source);

            direction.AddOnAxisListener(stick, source); // SteamVR_Input.GetVector2()
        }
    }

    public void stick(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
    {
        if (!click1.state && !click2.state) return;

        if (author == null) author = "?! VR Hands error ?!";
        object[] content = new object[] {author, axis.x, axis.y};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.TRACK_TargetMove, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void Catch(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (target != null && !caught /*&& transform != target.transform*/)
        {
            //target.photonView.TransferOwnership (PhotonNetwork.LocalPlayer) ;
            //target.photonView.RPC("ShowCaught", RpcTarget.All) ;
            //PhotonNetwork.SendAllOutgoingCommands () ;
            caught = true;
            target.Click();
        }
    }

    public void Release(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (caught) {
            //target.photonView.RPC("ShowReleased", RpcTarget.All) ;
            //PhotonNetwork.SendAllOutgoingCommands () ;
            caught = false ;
            target.Release();
            target = null ;
        }
    }

    void OnTriggerEnter (Collider other) {
        if (! caught) {
            target = other.gameObject.GetComponent<Interactive>();
            if (target != null) {
                //target.photonView.RPC ("ShowCatchable", RpcTarget.All) ;
                //PhotonNetwork.SendAllOutgoingCommands () ;
            }
        }
    }

    void OnTriggerStay (Collider other) {
        if (! caught) {
            if (target == null) {
                target = other.gameObject.GetComponent<Interactive>();
            }
        }
    }

    void OnTriggerExit (Collider other) {
        if (! caught) {
            if (target != null) {
                //target.photonView.RPC ("HideCatchable", RpcTarget.All) ;
                //PhotonNetwork.SendAllOutgoingCommands () ;
                target = null ;
            }
        }
    }
}
