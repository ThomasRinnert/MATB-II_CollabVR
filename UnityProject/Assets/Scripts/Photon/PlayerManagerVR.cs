using UnityEngine;
//using UnityEngine.XR;
using Valve.VR;
using UnityEngine.EventSystems;

using Photon.Pun;
using Photon.Pun.Demo.PunBasics;

using System.Collections;

public class PlayerManagerVR : PlayerManager, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //...
        }
        else
        {
            //...
        }
    }
    
    [SerializeField]
    public Vector3 cameraPositionOffset;
    [SerializeField]
    public Quaternion cameraOrientationOffset;
    [SerializeField]
    //public InputDevice device;
    public SteamVR_Action_Boolean SteamVR_bool = null;
    public SteamVR_Action_Vector2 joystick = null;

    [SerializeField]
    SteamVR_Behaviour_Pose[] hands;

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        isVR = true;
        // #Important
        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine) /* || ! */
        {
            PlayerManager.LocalPlayerInstance = this;
        }
        // #Critical
        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        if(PhotonNetwork.IsConnected) DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        if (GameManager.Instance != null) GameManager.Instance.players.Add(this);
        #if UNITY_5_4_OR_NEWER
        // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        #endif

        print(PhotonNetwork.NickName + " stole the cam !");
        if (Camera.main != null) Camera.main.enabled = false;
        Camera cam = GetComponentInChildren<Camera>();
        if(cam == null) cam = Camera.main;
        cameraTransform = cam.transform;
        cam.enabled = true;

        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            foreach (SteamVR_Behaviour_Pose hand in hands)
            {
                hand.enabled = true;
            }
        }
        if (photonView.IsMine)
        {
            /*
            Transform targetHead = GameObject.Find("HeadTarget").transform;
            targetHead.SetParent(cameraTransform, false);

            Transform targetL = GameObject.Find("LeftHandTarget").transform;
            Transform controllerL = GameObject.Find("Controller (left)").transform;
            targetL.SetParent(controllerL, false);

            Transform targetR = GameObject.Find("RightHandTarget").transform;
            Transform controllerR = GameObject.Find("Controller (right)").transform;
            targetR.SetParent(controllerR, false);
            */

            //device = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
        }
    }

    #if UNITY_5_4_OR_NEWER
    public override void OnDisable()
    {
        // Always call the base to remove callbacks
        base.OnDisable ();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }
    #endif

    #if !UNITY_5_4_OR_NEWER
    /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
    void OnLevelWasLoaded(int level)
    {
        this.CalledOnLevelWasLoaded(level);
    }
    #endif

    void CalledOnLevelWasLoaded(int level)
    {
        /*
        // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }
        if (PlayerManager.LocalPlayerInstance == null)
        {
            Destroy(this);
        }
        */
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity on every frame.
    /// </summary>
    void Update()
    {
        if (photonView.IsMine)
        {
            //device.IsPressed(InputHelpers.Button.PrimaryButton, out isPressed); // X button
            if (SteamVR_bool.state)
            {
                Camera theCamera = (Camera)this.gameObject.GetComponentInChildren (typeof(Camera)) ;
                theCamera.gameObject.SetActive(false);
                theCamera.gameObject.SetActive(true);
                theCamera.enabled = true;
            }
            /* NAVIGATION ?
            // translation
            device.IsPressed(InputHelpers.Button.PrimaryAxis2DUp, out isPressed, 0.3f); // Joystick
            if (isPressed)
            {
                transform.Translate(0, 0, deltaT);
            }
            device.IsPressed(InputHelpers.Button.PrimaryAxis2DDown, out isPressed, 0.3f); // Joystick
            if (isPressed)
            {
                transform.Translate(0, 0, -deltaT);
            }
            // rotation
            device.IsPressed(InputHelpers.Button.PrimaryAxis2DRight, out isPressed, 0.3f); // Joystick
            if (isPressed)
            {
                transform.Rotate(0, deltaR, 0);
            }
            device.IsPressed(InputHelpers.Button.PrimaryAxis2DLeft, out isPressed, 0.3f); // Joystick
            if (isPressed)
            {
                transform.Rotate(0, -deltaR, 0);
            }
            */
        }
    }
}