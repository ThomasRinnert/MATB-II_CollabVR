using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static PlayerManager LocalPlayerInstance;
    
    [SerializeField]
    public Transform cameraTransform;

    [SerializeField]
    public bool isVR;

    public string NickName
    {
        get {
            return GetComponentInParent<PhotonView>().Owner.NickName;
        }
    }

}
