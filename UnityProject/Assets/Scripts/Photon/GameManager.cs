using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    
    [Tooltip("The prefab to use for representing a desktop user")]
    public GameObject desktopPrefab;
    
    [Tooltip("The prefab to use for representing a VR user")]
    public GameObject VRPrefab;

    [SerializeField]
    public Vector3 spawnOffset = new Vector3(0f, 0f, 0f);

    [SerializeField]
    public List<PlayerManager> players = new List<PlayerManager>();

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient);
        }
    }

    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        PhotonNetwork.LogLevel = PunLogLevel.Full;

        //string userPrefab = PlayerPrefs.GetString("UserPrefab", "DesktopUser");
        bool isVR = PhotonNetwork.NickName.StartsWith("VR_");
        GameObject prefab = isVR ? VRPrefab : desktopPrefab;

        if (prefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
        }
        else
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                GameObject player = PhotonNetwork.Instantiate(prefab.name, spawnOffset, Quaternion.identity, 0);
                player.name = PhotonNetwork.NickName;
            }
            else
            {
                Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
            }
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}