using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields

    /// <summary>
    /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
    /// </summary>
    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    [SerializeField]
    private byte maxPlayersPerRoom = 10;

    [Tooltip("Scene to load as first player")]
    [SerializeField]
    private string SceneToLoad;

    [SerializeField]
    private bool isVR;
    [SerializeField]
    private bool isXP;
    public void setXP(bool b) {isXP = b; return;}

    [Tooltip("The Ui Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject controlPanel;
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject progressLabel;

    #endregion

    #region Private Fields


    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion.
    /// </summary>
    string gameVersion = "1.0";

    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    bool isConnecting;

    #endregion

    #region MonoBehaviour CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if(!isVR) XRSettings.enabled = false;
    }


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        //Connect();
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);   
    }


    #endregion

    #region Public Methods


    [ContextMenu("CONNECT")]
    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        /*
        ExitGames.Client.Photon.Hashtable table = new ExitGames.Client.Photon.Hashtable();
        table.Add("UserPrefab", gameObject.name);
        PhotonNetwork.SetPlayerCustomProperties(table);
        string PlayerName = PlayerPrefs.GetString("PlayerName", "U4");
        PlayerPrefs.SetString("UserPrefab", isVR ? "VRUser" : "DesktopUser");
        */

        if (!PhotonNetwork.NickName.Contains("_"))
        {
            if (isXP) PhotonNetwork.NickName = "XP_" + PhotonNetwork.NickName;
            else if (isVR) PhotonNetwork.NickName = "VR_" + PhotonNetwork.NickName;
            else PhotonNetwork.NickName = "Dsktp_" + PhotonNetwork.NickName;
        }
        
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN");

        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
        // we don't want to do anything.
        if (isConnecting)
        {
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = maxPlayersPerRoom});
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Joined Room");
        /*
        var p = FindObjectOfType<PlayerManager>();
        if (p != null) Destroy(p);
        */

        // #Critical: We only load if we are the first player, else we rely on `PhotonNetwork.AutomaticallySyncScene` to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            // #Critical
            // Load the Room Level.
            PhotonNetwork.LoadLevel(SceneToLoad);
        }
    }


    #endregion
}

#if UNITY_EDITOR
[CustomEditor(typeof(Launcher)), CanEditMultipleObjects]
public class LauncherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Launcher script = (Launcher)target;

        if(GUILayout.Button("Connect"))
        {
            script.Connect();
        }
    }
}
#endif