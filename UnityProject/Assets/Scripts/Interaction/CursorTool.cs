using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CursorTool : MonoBehaviourPun, IPunObservable
{
    private bool caught ;
    private Interactive target ;
    //public Interactive interactiveObjectToInstanciate ;
    void Start () {
        caught = false ;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(displayed);
        }
        else
        {
            bool display = (bool) stream.ReceiveNext();
            if (display != displayed)
            {
                displayed = display;
                Cursor_ShowHide();
                // photonView.RPC("Cursor_ShowHide", RpcTarget.All);
                // PhotonNetwork.SendAllOutgoingCommands();
            }
        }
    }
    
    private bool displayed = true;
    public void Display(bool display)
    {
        displayed = display;
        Cursor_ShowHide();
    }
    [PunRPC] public void Cursor_ShowHide()
    {
        GetComponent<Renderer>().enabled = displayed;
    }
    
    public void Catch()
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

    public void Release()
    {
        if (caught) {
            //target.photonView.RPC("ShowReleased", RpcTarget.All) ;
            //PhotonNetwork.SendAllOutgoingCommands () ;
            caught = false ;
            target.Release();
        }
    }

    public void fakeAxis(float x, float y, string author)
    {
        object[] content = new object[] {author, x, y};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.TRACK_TargetMove, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void CreateInteractiveCube () {
        //var objectToInstanciate = PhotonNetwork.Instantiate (interactiveObjectToInstanciate.name, transform.position, transform.rotation, 0) ;
    }

    void OnTriggerEnter (Collider other) {
        if (! caught) {
            // print (name + " : CursorTool OnTriggerEnter") ;
            target = other.gameObject.GetComponent<Interactive>();
            if (target != null) {
                //target.photonView.RPC ("ShowCatchable", RpcTarget.All) ;
                //PhotonNetwork.SendAllOutgoingCommands () ;
            }
        }
    }

    void OnTriggerExit (Collider other) {
        if (! caught) {
            // print (name + " : CursorTool OnTriggerExit") ;
            if (target != null) {
                //target.photonView.RPC ("HideCatchable", RpcTarget.All) ;
                //PhotonNetwork.SendAllOutgoingCommands () ;
                target = null ;
            }
        }
    }

}
