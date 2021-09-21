using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CursorDriver : MonoBehaviourPun
{
    private CursorTool cursor ;
    private Camera cam ;
    private PlayerManager playerManager;

    void Start () {
        if (photonView.IsMine || ! PhotonNetwork.IsConnected) {
            // get the camera
            playerManager = transform.GetComponentInParent<PlayerManager>();
            cam = playerManager.cameraTransform.GetComponent<Camera>();
            cursor = GetComponent<CursorTool> () ;
        }
        else
        {
            GetComponent<Collider>().enabled = false;
            gameObject.tag = "Untagged";
        }
    }
    
    void Update () {
        if (photonView.IsMine  || ! PhotonNetwork.IsConnected) {
            if (Input.GetButtonDown ("Fire1"))  {
                cursor.Catch () ;
            }
            if (Input.GetButtonUp ("Fire1")) {
                cursor.Release () ;
            }
            /*
            if (Input.GetKeyDown (KeyCode.C)) {
                cursor.CreateInteractiveCube () ;
            }
            */
            if (Input.mousePosition != null) {
                Vector3 point = new Vector3 () ;
                Vector3 mousePos = Input.mousePosition ;
                float deltaZ = Input.mouseScrollDelta.y / 20.0f ;
                cursor.transform.Translate (0, 0, deltaZ) ;
                // Note that the y position from Event should be inverted, but maybe it is not true any longer...
                // mousePos.y = myCamera.pixelHeight - mousePos.y ;
                point = cam.ScreenToWorldPoint (new Vector3 (mousePos.x, mousePos.y, cursor.transform.localPosition.z - cam.transform.localPosition.z)) ;
                cursor.transform.position = point ;
            }
            
            if (Input.GetKeyDown("[1]")) {
                cursor.fakeAxis( -1.0f, -1.0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[2]")) {
                cursor.fakeAxis( .0f, -1.0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[3]")) {
                cursor.fakeAxis( 1.0f, -1.0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[4]")) {
                cursor.fakeAxis( -1.0f, .0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[5]")) {
                cursor.fakeAxis( .0f, .0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[6]")) {
                cursor.fakeAxis( 1.0f, .0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[7]")) {
                cursor.fakeAxis( -1.0f, 1.0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[8]")) {
                cursor.fakeAxis( .0f, 1.0f, playerManager.NickName);
            }
            if (Input.GetKeyDown("[9]")) {
                cursor.fakeAxis( 1.0f, 1.0f, playerManager.NickName);
            }
        }
    }
}
