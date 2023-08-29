using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static PlayerManager LocalPlayerInstance;

    [SerializeField]
    public string overrideName = "";
    
    [SerializeField]
    public Transform cameraTransform;
    public Camera playerCamera;

    [SerializeField]
    public bool isVR;

    [SerializeField]
    public int workload_cog = 0;
    public int workload_phys = 0;

    public Player player
    {
        get {
            return GetComponentInParent<PhotonView>().Owner;
        }
    }

    public string NickName
    {
        get {
            if(overrideName != "") return overrideName;
            else return GetComponentInParent<PhotonView>().Owner.NickName;
        }
    }
}
