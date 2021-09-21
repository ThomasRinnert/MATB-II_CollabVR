using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CursorTool : MonoBehaviour
{
    private bool caught ;
    private Interactive target ;
    //public Interactive interactiveObjectToInstanciate ;
    void Start () {
        caught = false ;
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
        PhotonNetwork.RaiseEvent(MATBIISystem.TRACK_TargetMove, content, raiseEventOptions, SendOptions.SendReliable);
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
