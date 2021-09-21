﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MATBIISystem : MonoBehaviourPun, IPunObservable, IOnEventCallback
{
    #region SINGLETON PATTERN
    private static MATBIISystem _instance;
    public static MATBIISystem Instance
    {
        get {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<MATBIISystem>();
                
                /*
                if (_instance == null)
                {
                    GameObject container = new GameObject("MATBIISystem");
                    _instance = container.AddComponent<MATBIISystem>();
                }
                */
            }
            return _instance;
        }
    }
    #endregion

    public enum MATBII_TASK : int
    {
        SYSMON = 0,
        COMM = 1,
        TRACK = 2,
        RESMAN = 3
    }

    #region SYSMON_Attributes
    [Header("SYSMON")]
    
    [SerializeField] [Tooltip("Seconds before task failure")] [Range(0.0f, 60.0f)]
    public float SYSMON_timeLimit = 15.0f; // Failure condition

    private bool SYSMON_TASK_active = false;
    public bool isSYSMON_TASK_active() { return SYSMON_TASK_active; }

    private bool SYSMON_NormallyON_active = false;
    public bool isSYSMON_NormallyON_active() { return SYSMON_NormallyON_active; }
    private float SYSMON_NormallyON_timer = 0.0f;
    public float getSYSMON_NormallyON_timer() { return SYSMON_NormallyON_timer; }
    private bool SYSMON_NormallyOFF_active = false;
    public bool isSYSMON_NormallyOFF_active() { return SYSMON_NormallyOFF_active; }
    private float SYSMON_NormallyOFF_timer = 0.0f;
    public float getSYSMON_NormallyOFF_timer() { return SYSMON_NormallyOFF_timer; }

    private bool[] SYSMON_Scales_active = {false, false, false, false};
    public bool isSYSMON_Scales_active() { return SYSMON_Scales_active[0] || SYSMON_Scales_active[1] || SYSMON_Scales_active[2] || SYSMON_Scales_active[3]; }
    private bool[] SYSMON_Scales_upward = {true, true, true, true};
    private float[] SYSMON_Scales_timers = {0.0f, 0.0f, 0.0f, 0.0f};
    public float[] getSYSMON_Scales_timers() { return SYSMON_Scales_timers; }
    private float[] SYSMON_Scales_criticalTimers = {0.0f, 0.0f, 0.0f, 0.0f};
    public float[] getSYSMON_Scales_criticalTimers() { return SYSMON_Scales_criticalTimers; }
    #endregion

    #region COMM_Attributes
    [Header("COMM")]
    
    [SerializeField] [Tooltip("Seconds before task failure")] [Range(0.0f, 60.0f)]
    public float COMM_timeLimit = 20.0f; // Failure condition

    public enum COMM_Radio : int { NAV1 = 0, NAV2 = 1, COM1 = 2, COM2 = 3 }
    private string[] COMM_radioNames = {"NAV1", "NAV2", "COM1", "COM2"};
    private float[] COMM_MIN_frequencies = {108.000f, 108.000f, 118.000f, 118.000f};
    private float[] COMM_MAX_frequencies = {118.000f, 118.000f, 136.000f, 136.000f};
    private float[] COMM_increments = {0.050f, 0.050f, 0.025f, 0.025f};

    [System.Serializable]
    public class AudioTransmission
    {
        public string name;
        public bool isNASA504;
        public int radio;
        public float frequency;
        public AudioClip clip;
    }
    [SerializeField]
    public List<AudioTransmission> COMM_transmissions = new List<AudioTransmission>();

    private bool COMM_TASK_active = false; //redundant with COMM_onGoingTransmission != null ?
    public bool isCOMM_TASK_active() { return COMM_TASK_active; }
    private AudioTransmission COMM_onGoingTransmission = null;
    private Coroutine COMM_playing = null;
    
    private float[] COMM_frequencies = {108.0f, 108.0f, 118.0f, 118.0f};
    private string[] COMM_latestModif = {"none", "none", "none", "none"};
    private bool COMM_responded = false;
    private float COMM_responseTimer = 0.0f;
    private float COMM_completionTimer = 0.0f;
    public float getCOMM_timer() { return COMM_responseTimer + COMM_completionTimer; }
    #endregion

    #region TRACK_Attributes
    [Header("TRACK")]
    
    [SerializeField] [Tooltip("Target moving speed")] [Range(0.0f, 15.0f)]
    public float TRACK_speed = 1.0f;

    [SerializeField] [Tooltip("Duration of the task")] [Range(0.0f, 120.0f)]
    public float TRACK_duration = 30.0f;

    private bool TRACK_TASK_active = false;
    public bool isTRACK_TASK_active() { return TRACK_TASK_active; }

    private float TRACK_timer = 0.0f;
    public float getTRACK_timer() { return TRACK_timer; }
    private float TRACK_successTimer = 0.0f;
    public float getTRACK_successTimer() { return TRACK_successTimer; }
    #endregion

    #region RESMAN_Attributes
    
    public enum RESMAN_Tank : int
    { A = 0, B = 1, C = 2, D = 3, E = 4, F = 5 }

    public enum RESMAN_Pump : int
    { AToB = 0, BToA = 1, CToA = 2, DToA = 3, DToC = 4, EToB = 5, FToB = 6, FToE = 7 }
    
    public enum RESMAN_PumpState : int
    { Active = 0, Inactive = 1, Failed = 2 }
   
    [Header("RESMAN")]
    [SerializeField] [Tooltip("Optimal capacity of tanks")] [Range(2000.0f, 3000.0f)]
    public float RESMAN_objective = 2500.0f;
    [SerializeField] [Tooltip("Duration of the task")] [Range(0.0f, 120.0f)]
    public float RESMAN_duration = 30.0f;
    [SerializeField] [Tooltip("Probability of pump failure")] [Range(0.0f, 10.0f)]
    public float RESMAN_pumpFailChance = 1.0f;
    [SerializeField] [Tooltip("Factor applied to flow/consumption rates")] [Range(1.0f, 5.0f)]
    public float RESMAN_speedFactor = 2.0f;

    //[SerializeField] [Tooltip("Fuel consumption of tank A")] [Range(1.0f, 15.0f)]
    private float RESMAN_tankAConsumption = 13.33f; // NASA default: 800/min
    //[SerializeField] [Tooltip("Fuel consumption of tank B")] [Range(1.0f, 15.0f)]
    private float RESMAN_tankBConsumption = 13.33f; // NASA default: 800/min
    private Dictionary<RESMAN_Pump, float> RESMAN_PumpRates = new Dictionary<RESMAN_Pump, float>()
    {
        [RESMAN_Pump.AToB] =  6.66f, // = NASA default: 400/min
        [RESMAN_Pump.BToA] =  6.66f, // = NASA default: 400/min
        [RESMAN_Pump.CToA] = 13.33f, // = NASA default: 800/min
        [RESMAN_Pump.DToA] = 10.00f, // = NASA default: 600/min
        [RESMAN_Pump.DToC] = 10.00f, // = NASA default: 600/min
        [RESMAN_Pump.EToB] = 13.33f, // = NASA default: 800/min
        [RESMAN_Pump.FToB] = 10.00f, // = NASA default: 600/min
        [RESMAN_Pump.FToE] = 10.00f  // = NASA default: 600/min
    };

    private bool RESMAN_TASK_active = false;
    public bool isRESMAN_TASK_active() { return RESMAN_TASK_active; }

    private float[] RESMAN_capacity = {4000.0f, 4000.0f, 2000.0f, float.MaxValue, 2000.0f, float.MaxValue};
    public float getRESMAN_capacity(RESMAN_Tank t) { return RESMAN_capacity[(int)t]; }
    private float[] RESMAN_tanks = {2500.0f, 2500.0f, 1500.0f, float.MaxValue, 1500.0f, float.MaxValue};
    public float getRESMAN_tank(RESMAN_Tank t) { return RESMAN_tanks[(int)t]; }
    private float RESMAN_timer = 0.0f;
    private float RESMAN_optimalTimer = 0.0f;

    private RESMAN_PumpState[] RESMAN_pumpStates = {RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive, RESMAN_PumpState.Inactive};
    private float[] RESMAN_pumpTimers = {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
    #endregion

    [SerializeField] public List<MATBIIInterface> interfaces;
    public int sessionID = 0;
    public string logSessionFolder;
    public System.Globalization.CultureInfo strFormat = System.Globalization.CultureInfo.InvariantCulture;
    public double elapsedTime;
    public bool started = false;

    // Photon Events code
    public const byte SYSMON_SwitchPressed = 1;
    public const byte SYSMON_ScalePressed = 2;
    public const byte COMM_RadioChanged = 3;
    public const byte TRACK_TargetMove = 4;
    public const byte RESMAN_PumpChanged = 5;

    // IPunObservable callback
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(elapsedTime);
            stream.SendNext(SYSMON_NormallyON_active);
            stream.SendNext(SYSMON_NormallyON_timer);

            stream.SendNext(SYSMON_NormallyOFF_active);
            stream.SendNext(SYSMON_NormallyOFF_timer);
            
            for (int i = 0; i < 4; i++)
            {
                stream.SendNext(SYSMON_Scales_active[i]);
                stream.SendNext(SYSMON_Scales_timers[i]);
                stream.SendNext(SYSMON_Scales_criticalTimers[i]);
            }

            stream.SendNext(COMM_TASK_active);
            stream.SendNext(COMM_responseTimer);
            stream.SendNext(COMM_completionTimer);

            stream.SendNext(TRACK_TASK_active);
            stream.SendNext(TRACK_timer);
            stream.SendNext(TRACK_successTimer);

            stream.SendNext(RESMAN_TASK_active);
            stream.SendNext(RESMAN_tanks[0]);
            stream.SendNext(RESMAN_tanks[1]);
            stream.SendNext(RESMAN_tanks[2]);
            stream.SendNext(RESMAN_tanks[4]);
        }
        else
        {
            this.elapsedTime               = (double) stream.ReceiveNext();
            this.SYSMON_NormallyON_active  = (bool)   stream.ReceiveNext();
            this.SYSMON_NormallyON_timer   = (float)  stream.ReceiveNext();

            this.SYSMON_NormallyOFF_active = (bool)   stream.ReceiveNext();
            this.SYSMON_NormallyOFF_timer  = (float)  stream.ReceiveNext();

            for (int i = 0; i < 4; i++)
            {
                this.SYSMON_Scales_active[i]         = (bool)  stream.ReceiveNext();
                this.SYSMON_Scales_timers[i]         = (float) stream.ReceiveNext();
                this.SYSMON_Scales_criticalTimers[i] = (float) stream.ReceiveNext();
            }

            this.COMM_TASK_active     = (bool)  stream.ReceiveNext();
            this.COMM_responseTimer   = (float) stream.ReceiveNext();
            this.COMM_completionTimer = (float) stream.ReceiveNext();

            this.TRACK_TASK_active    = (bool)  stream.ReceiveNext();
            this.TRACK_timer          = (float) stream.ReceiveNext();
            this.TRACK_successTimer   = (float) stream.ReceiveNext();

            this.RESMAN_TASK_active   = (bool)  stream.ReceiveNext();
            this.RESMAN_tanks[0]      = (float) stream.ReceiveNext();
            this.RESMAN_tanks[1]      = (float) stream.ReceiveNext();
            this.RESMAN_tanks[2]      = (float) stream.ReceiveNext();
            this.RESMAN_tanks[4]      = (float) stream.ReceiveNext();
        }
    }

    // IOnEventCallback
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        
        if (eventCode == SYSMON_SwitchPressed)
        {
            if (!SYSMON_TASK_active) return;

            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";
            bool normallyON = (bool)data[1];
            
            if (normallyON && SYSMON_NormallyON_active && photonView.IsMine)
            {
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, author, "Normally ON Light", true, SYSMON_NormallyON_timer.ToString("0.0000", strFormat));
                photonView.RPC("SYSMON_normallyON_Reset", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            else if (!normallyON && SYSMON_NormallyOFF_active && photonView.IsMine)
            {
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, author, "Normally OFF Light", true, SYSMON_NormallyOFF_timer.ToString("0.0000", strFormat));
                photonView.RPC("SYSMON_normallyOFF_Reset", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            return;
        }

        if (eventCode == SYSMON_ScalePressed)
        {
            if (!SYSMON_TASK_active) return;

            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";
            int index = (int)data[1];
            
            if (SYSMON_Scales_active[index] && photonView.IsMine)
            {
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, author, "Scale", true, SYSMON_Scales_timers[index].ToString("0.0000", strFormat) + "(" + SYSMON_Scales_criticalTimers[index].ToString("0.0000", strFormat) + ")");
                photonView.RPC("SYSMON_scale_Reset", RpcTarget.All, index);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            return;
        }
        
        if (eventCode == COMM_RadioChanged)
        {
            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";
            int r = (int)data[1]; COMM_Radio radio = (COMM_Radio)r;
            float frequency = (float)data[2];

            /*
            if (!COMM_TASK_active && photonView.IsMine)
            {
                photonView.RPC("COMM_SetFrequency", RpcTarget.All, r, COMM_frequencies[r]);
                PhotonNetwork.SendAllOutgoingCommands();
                return;
            }
            */

            COMM_responded = true;
            COMM_frequencies[r] = frequency;
            COMM_latestModif[r] = author;

            // if (author == PhotonNetwork.LocalPlayer.NickName) ?
            if (photonView.IsMine)
            {
                photonView.RPC("COMM_SetFrequency", RpcTarget.All, r, frequency);
                PhotonNetwork.SendAllOutgoingCommands();

                //Success
                if (COMM_onGoingTransmission.radio == r && COMM_onGoingTransmission.frequency == frequency && COMM_onGoingTransmission.isNASA504)
                {
                    photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.COMM, COMM_latestModif[r].ToString(), "Radio", true,
                        COMM_radioNames[COMM_onGoingTransmission.radio] + ", " + 
                        COMM_onGoingTransmission.frequency.ToString("0.000", strFormat) + ", " + 
                        COMM_radioNames[r] + ", " + 
                        frequency.ToString("0.000", strFormat) + ", " + 
                        COMM_responseTimer.ToString("0.0000", strFormat) + ", " + 
                        COMM_completionTimer.ToString("0.0000", strFormat));
                    photonView.RPC("COMM_Reset", RpcTarget.All);
                    PhotonNetwork.SendAllOutgoingCommands();
                }
            }
            return;
        }

        if (eventCode == TRACK_TargetMove)
        {
            object[] data = (object[])photonEvent.CustomData;
            if (data == null) return;

            string author = (string)data[0];
            float x = (float)data[1];
            float y = (float)data[2];

            if (photonView.IsMine)
            {
                photonView.RPC("TRACK_Move", RpcTarget.All, x, y);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            return;
        }

        if (eventCode == RESMAN_PumpChanged)
        {
            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";
            int p = (int)data[1]; RESMAN_Pump pump = (RESMAN_Pump)p;

            if (RESMAN_pumpStates[p] != RESMAN_PumpState.Failed && RESMAN_pumpTimers[p] > 0.5f && photonView.IsMine)
            {
                
                RESMAN_PumpState state = RESMAN_pumpStates[p] == RESMAN_PumpState.Active ? RESMAN_PumpState.Inactive : RESMAN_PumpState.Active;
                photonView.RPC("RESMAN_changePump", RpcTarget.All, p, state);
                string status = RESMAN_getStatus();
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.RESMAN, author, "Pump " + (state == RESMAN_PumpState.Active ? "activated" : "deactivated"), true, status);
                PhotonNetwork.SendAllOutgoingCommands();
            }

            return;
        }

        if (eventCode == 201 || eventCode == 206) { return; /* Serialized data */ }

        if (eventCode == 253)
        {
            /*
            Event code 253 is for PropertiesChanged event.
            You get this event when another remote player has changed a room or player property (with the broadcast option "on" which is the default behaviour).
            You can implement PUN callbacks:
            - void OnPhotonCustomRoomPropertiesChanged (Hashtable propertiesThatChanged)
            - void OnPhotonPlayerPropertiesChanged (object[] playerAndUpdatedProps)
            */
            return;
        }

        Debug.LogWarning("Unhandled EventData from photon... event code: " + eventCode.ToString());
        return;
    }

    void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        CreateLogFiles();

        //COMM_LoadAudioResources();
        for (int radio = 0; radio < COMM_frequencies.Length; radio++)
        {
            foreach (var i in interfaces)
            {
                i.COMM_radios[radio].SetFrequency(COMM_frequencies[radio]);
            }
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Update()
    {
        if (!started || !photonView.IsMine) return;

        elapsedTime += Time.deltaTime;

        if(SYSMON_TASK_active) { UpdateSYSMON(); }

        if(COMM_TASK_active) { UpdateCOMM(); }

        if(TRACK_TASK_active) { UpdateTRACK(); }

        if(started /* RESMAN_TASK_active */) { UpdateRESMAN(); }
    }

    private void CreateLogFiles()
    {
        logSessionFolder = Application.dataPath + "/Logs_session_" + sessionID + '/';
        try { if (!Directory.Exists(logSessionFolder)) { Directory.CreateDirectory(logSessionFolder); } }
        catch (IOException ex) { Debug.LogError(ex.Message); }

        string baseHeader = "Time, User ID, Task, Event, Sucess, ";
        
        string SYSMON_path = logSessionFolder + "SYSMON_" + sessionID + ".csv";
        string SYSMON_log_header = baseHeader + "Response Time (Critical Time)\n";
        File.WriteAllText(SYSMON_path, SYSMON_log_header);

        string COMM_path = logSessionFolder + "COMM_"   + sessionID + ".csv";
        string COMM_log_header = baseHeader + "Target Channel, Target Frequency, Modified Channel, Modified Frequency, Response Time, Completion Time\n";
        File.WriteAllText(COMM_path, COMM_log_header);
        
        string TRACK_path = logSessionFolder + "TRACK_"  + sessionID + ".csv";
        string TRACK_log_header = baseHeader + "Inner zone time ratio (%)\n";
        File.WriteAllText(TRACK_path, TRACK_log_header);
        
        string RESMAN_path = logSessionFolder + "RESMAN_" + sessionID + ".csv";
        string RESMAN_log_header = baseHeader + "Tank A, Tank B, Tank C, Tank E, Pump A->B, Pump B->A, Pump C->A, Pump D->A, Pump D->C, Pump E->B, Pump F->B, Pump F->E, time spend in optimal range, ratio (%)\n";
        File.WriteAllText(RESMAN_path, RESMAN_log_header);
        
        string WRS_path = logSessionFolder + "WRS_" + sessionID + ".csv";
        string WRS_log_header = baseHeader + "Mental, Physical, Temporal, Perfomance, Effort, Frustration\n";
        File.WriteAllText(WRS_path, WRS_log_header);
    }

    [PunRPC]
    private void LogEvent(MATBII_TASK tag, string userID, string eventID, bool succes, string data)
    {
        string task = "";
        switch (tag)
        {
            case MATBII_TASK.SYSMON:
                task = "SYSMON";
                break;
            case MATBII_TASK.COMM:
                task = "COMM";
                break;
            case MATBII_TASK.TRACK:
                task = "TRACK";
                break;
            case MATBII_TASK.RESMAN:
                task = "RESMAN";
                break;
            default:
                break;
        }

        string time = elapsedTime.ToString("0.0", strFormat);
        string text = time + ", " + userID + ", " + task + ", " + eventID + ", " + succes + ", " + data + '\n';
        File.AppendAllText(logSessionFolder + task + "_" + sessionID + ".csv", text);
    }

    #region SYSMON
    private void UpdateSYSMON()
    {
        //NormallyON Switch
        if (SYSMON_NormallyON_active)
        {
            SYSMON_NormallyON_timer += Time.deltaTime;
            //Failure: Time Limit
            if(SYSMON_NormallyON_timer > SYSMON_timeLimit)
            {
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Normally ON Light", false, SYSMON_NormallyON_timer.ToString("0.0000", strFormat));
                photonView.RPC("SYSMON_normallyON_Reset", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }
        //NormallyOFF Switch
        if (SYSMON_NormallyOFF_active)
        {
            SYSMON_NormallyOFF_timer += Time.deltaTime;
            //Failure: Time Limit
            if(SYSMON_NormallyOFF_timer > SYSMON_timeLimit)
            {
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Normally OFF Light", false, SYSMON_NormallyOFF_timer.ToString("0.0000", strFormat));
                photonView.RPC("SYSMON_normallyOFF_Reset", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }
        //Scales
        for (int s = 0; s < 4; s++)
        {
            if (SYSMON_Scales_active[s])
            {
                if (interfaces[0].SYSMON_Scales[s].isOutOfBounds()) { SYSMON_Scales_criticalTimers[s] += Time.deltaTime; }
                else SYSMON_Scales_timers[s] += Time.deltaTime;
                
                //Failure
                if (SYSMON_Scales_criticalTimers[s] > SYSMON_Scales_timers[s])
                {
                    photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Scale", false, SYSMON_Scales_timers[s].ToString("0.0000", strFormat) + "(" + SYSMON_Scales_criticalTimers[s].ToString("0.0000", strFormat) + ")");
                    photonView.RPC("SYSMON_scale_Reset", RpcTarget.All, s);
                    PhotonNetwork.SendAllOutgoingCommands();
                    break;
                }
            }
        }
    }

    [PunRPC]
    public void SYSMON_Start(bool[] direction, PhotonMessageInfo info)
    {
        SYSMON_TASK_active = true;

        SYSMON_normallyON_Start();
        SYSMON_normallyOFF_Start();
        for (int i = 0; i < 4; i++) SYSMON_scale_Start(i,direction[i]);
    }

    [PunRPC]
    public void SYSMON_Reset()
    {
        SYSMON_normallyON_Reset();
        SYSMON_normallyOFF_Reset();
        for (int i = 0; i < 4; i++) SYSMON_scale_Reset(i);

        SYSMON_TASK_active = false;
    }

    [PunRPC]
    public void SYSMON_normallyON_Start()
    {
        if (SYSMON_NormallyON_active && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Normally ON Light", false, SYSMON_NormallyON_timer.ToString("0.0000", strFormat));
            photonView.RPC("SYSMON_normallyON_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        SYSMON_TASK_active = true;
        SYSMON_NormallyON_active = true;
        SYSMON_NormallyON_timer = 0.0f;
        foreach (var i in interfaces)
        {
            i.SYSMON_normallyON_Switch.Activate();
        }
    }

    [PunRPC]
    public void SYSMON_normallyON_Reset()
    {
        SYSMON_NormallyON_timer = 0.0f;
        SYSMON_NormallyON_active = false;
        foreach (var i in interfaces)
        {
            i.SYSMON_normallyON_Switch.Restore();
        }

        SYSMON_TASK_active = SYSMON_NormallyON_active || SYSMON_NormallyOFF_active || SYSMON_Scales_active[0] || SYSMON_Scales_active[1] || SYSMON_Scales_active[2] || SYSMON_Scales_active[3];
    }

    [PunRPC]
    public void SYSMON_normallyOFF_Start()
    {
        if (SYSMON_NormallyOFF_active && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Normally OFF Light", false, SYSMON_NormallyOFF_timer.ToString("0.0000", strFormat));
            photonView.RPC("SYSMON_normallyOFF_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        SYSMON_TASK_active = true;
        SYSMON_NormallyOFF_active = true;
        SYSMON_NormallyOFF_timer = 0.0f;
        foreach (var i in interfaces)
        {
            i.SYSMON_normallyOFF_Switch.Activate();
        }
    }

    [PunRPC]
    public void SYSMON_normallyOFF_Reset()
    {
        SYSMON_NormallyOFF_timer = 0.0f;
        SYSMON_NormallyOFF_active = false;
        foreach (var i in interfaces)
        {
            i.SYSMON_normallyOFF_Switch.Restore();
        }

        SYSMON_TASK_active = SYSMON_NormallyON_active || SYSMON_NormallyOFF_active || SYSMON_Scales_active[0] || SYSMON_Scales_active[1] || SYSMON_Scales_active[2] || SYSMON_Scales_active[3];
    }

    [PunRPC]
    public void SYSMON_scale_Start(int scale, bool direction)
    {
        if (SYSMON_Scales_active[scale] && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Scale", false, SYSMON_Scales_timers[scale].ToString("0.0000", strFormat) + "(" + SYSMON_Scales_criticalTimers[scale].ToString("0.0000", strFormat) + ")");
            photonView.RPC("SYSMON_scale_Reset", RpcTarget.All, scale);
            PhotonNetwork.SendAllOutgoingCommands();
        }

        SYSMON_TASK_active = true;
        SYSMON_Scales_active[scale] = true;
        SYSMON_Scales_upward[scale] = direction; //Random.Range(float.MinValue, float.MaxValue) > 0.0f;
        foreach (var i in interfaces)
        {
            i.SYSMON_Scales[scale].Move(SYSMON_timeLimit, SYSMON_Scales_upward[scale]);
        }
    }

    [PunRPC]
    public void SYSMON_scale_Reset(int scale)
    {
        SYSMON_Scales_timers[scale] = 0.0f;
        SYSMON_Scales_criticalTimers[scale] = 0.0f;
        SYSMON_Scales_active[scale] = false;
        foreach (var i in interfaces)
        {
            i.SYSMON_Scales[scale].ResetPos();
        }

        SYSMON_TASK_active = SYSMON_NormallyON_active || SYSMON_NormallyOFF_active || SYSMON_Scales_active[0] || SYSMON_Scales_active[1] || SYSMON_Scales_active[2] || SYSMON_Scales_active[3];
    }
    #endregion

    #region COMM
    private void UpdateCOMM()
    {
        if (!COMM_responded) { COMM_responseTimer += Time.deltaTime; }
        else { COMM_completionTimer += Time.deltaTime; }

        if (COMM_responseTimer + COMM_completionTimer > COMM_timeLimit)
        {
            photonView.RPC("COMM_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    [ContextMenu("Update COMM transmissions")]
    private void COMM_LoadAudioResources()
    {
        COMM_transmissions = new List<AudioTransmission>();
        
        DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/Resources/COMM_transmissions/");
        if (!info.Exists) return;
        FileInfo[] files = info.GetFiles("*.wav");
        for (int f = 0; f < files.Length; f++)
        {
            string[] filename = files[f].Name.Split('.');
            if (filename[filename.Length-1].Contains("meta")) continue;
            string transmission = filename[0];

            AudioTransmission audio = new AudioTransmission();
            audio.name = transmission;
            // Destination
            audio.isNASA504 = transmission.Contains("OWN");
            // Target Radio
            for (int i = 0; i < COMM_radioNames.Length; i++)
            {
                if (transmission.Contains(COMM_radioNames[i])) audio.radio = i;
            }
            //Target Frequency
            filename = transmission.Split('_');
            audio.frequency = float.Parse(filename[filename.Length-1].Replace('-', '.'), strFormat);

            audio.clip = Resources.Load<AudioClip>("COMM_transmissions/" + transmission);

            COMM_transmissions.Add(audio);
        }
    }

    private IEnumerator COMM_Play()
    {
        foreach (var i in interfaces)
        {
            i.COMM_audio.clip = COMM_onGoingTransmission.clip;
            i.COMM_audio.Play();
        }
        yield return new WaitForSeconds(COMM_onGoingTransmission.clip.length);
    }

    [PunRPC]
    public void COMM_Start(int index)
    {
        if (COMM_TASK_active) { COMM_Reset(); }

        COMM_TASK_active = true;
        COMM_responseTimer = 0.0f;
        COMM_completionTimer = 0.0f;
        COMM_onGoingTransmission = COMM_transmissions[index];

        COMM_playing = StartCoroutine(COMM_Play());
    }

    [PunRPC]
    public void COMM_Reset()
    {
        // Shutdown audio if active
        foreach (var i in interfaces)
        {
            i.COMM_audio.Stop();
        }
        if (COMM_playing != null)
        {
            StopCoroutine(COMM_playing);
            COMM_playing = null;
        }

        string modifiedByUser = "[ "; string modifiedRadios = "[ "; string modifiedFrequencies = "[ ";
        for (int i = 0; i < COMM_latestModif.Length; i++)
        {
            if (COMM_latestModif[i] != "none")
            {
                modifiedByUser += COMM_latestModif[i].ToString(strFormat) + " ";
                modifiedRadios += COMM_radioNames[i].ToString(strFormat) + " ";
                modifiedFrequencies += COMM_frequencies[i].ToString(strFormat) + " ";
            }
        }
        modifiedByUser += "]"; modifiedRadios += "]"; modifiedFrequencies += "]";

        //COMM Transmission was not adressed to users ("NASA 504")
        //Success: If no one changed any radio
        if(!COMM_onGoingTransmission.isNASA504 && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.COMM, modifiedByUser, "Radio (Not adressed to NASA 504)", !COMM_responded,
                COMM_radioNames[COMM_onGoingTransmission.radio] + ", " + 
                COMM_onGoingTransmission.frequency.ToString("0.000", strFormat) + ", " + 
                modifiedRadios + ", " + 
                modifiedFrequencies + ", " + 
                COMM_responseTimer.ToString("0.0000", strFormat) + ", " + 
                COMM_completionTimer.ToString("0.0000", strFormat));
            PhotonNetwork.SendAllOutgoingCommands();
        }

        //Failure: No one changed the target radio
        else if (COMM_latestModif[COMM_onGoingTransmission.radio] == "none" && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.COMM, modifiedByUser, "Radio", false,
                COMM_radioNames[COMM_onGoingTransmission.radio] + ", " + 
                COMM_onGoingTransmission.frequency.ToString("0.000", strFormat) + ", " + 
                modifiedRadios + ", " + 
                modifiedFrequencies + ", " + 
                COMM_responseTimer.ToString("0.0000", strFormat) + ", " + 
                COMM_completionTimer.ToString("0.0000", strFormat));
            PhotonNetwork.SendAllOutgoingCommands();
        }

        //Failure: Wrong channel on target radio
        else if(COMM_frequencies[COMM_onGoingTransmission.radio] != COMM_onGoingTransmission.frequency && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.COMM, COMM_latestModif[COMM_onGoingTransmission.radio], "Radio", false,
                COMM_radioNames[COMM_onGoingTransmission.radio] + ", " + 
                COMM_onGoingTransmission.frequency.ToString("0.0000", strFormat) + ", " + 
                COMM_radioNames[COMM_onGoingTransmission.radio] + ", " + 
                COMM_frequencies[COMM_onGoingTransmission.radio].ToString() + ", " + 
                COMM_responseTimer.ToString("0.0000", strFormat) + ", " + 
                COMM_completionTimer.ToString("0.0000", strFormat));
            PhotonNetwork.SendAllOutgoingCommands();
        }

        /* Should not be needed
        // Reset all interfaces
        for (int radio = 0; radio < COMM_frequencies.Length; radio++)
        {
            foreach (var i in interfaces)
            {
                i.COMM_radios[radio].SetFrequency(COMM_frequencies[radio]);
            }
        }
        */

        // Reset internal values
        COMM_TASK_active = false;
        COMM_onGoingTransmission = null;
        COMM_responded = false;
        for (int i = 0; i < 4; i++) COMM_latestModif[i] = "none";
        COMM_responseTimer = 0.0f;
        COMM_completionTimer = 0.0f;
    }

    [PunRPC]
    public void COMM_SetFrequency(int radio, float frequency)
    {
        for (int i = 0; i < interfaces.Count; i++)
        {
            interfaces[i].COMM_radios[radio].SetFrequency(frequency);
        }
    }
    #endregion

    //Still need to handle a selection tool with tag "Crosshair"
    #region TRACK
    private void UpdateTRACK()
    {
        if (TRACK_timer < TRACK_duration)
        {
            TRACK_timer += Time.deltaTime;
            foreach (var i in interfaces)
            {
                if (i.TRACK_target.inInnerZone())
                {
                    TRACK_successTimer += Time.deltaTime;
                    break;
                }
            }
            if (TRACK_timer % 5.0f < 0.01f && photonView.IsMine) // maybe rework the periodicity
            {
                photonView.RPC("TRACK_newdDirection", RpcTarget.All, Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }
        else
        {
            photonView.RPC("TRACK_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    [PunRPC]
    public void TRACK_Start(float x, float y)
    {
        if (TRACK_TASK_active) { TRACK_timer = 0.0f; return; } //we add time instead of starting a new task

        //Activate tracking tool
        //...

        foreach (var i in interfaces)
        {
            i.TRACK_AutomodeDisplay.gameObject.SetActive(false);
            i.TRACK_target.Activate();
        }

        TRACK_TASK_active = true;
        //TRACK_missedTarget = 0;
        TRACK_timer = 0.0f;
        TRACK_successTimer = 0.0f;

        if (photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.TRACK, "[ ]", "MANUAL MODE", true, "");
            photonView.RPC("TRACK_newdDirection", RpcTarget.All, x, y);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }

    [PunRPC]
    public void TRACK_Reset()
    {
        //Disable tracking tool
        //...

        if (photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.TRACK, "[ ]", "MANUAL MODE", false, ((TRACK_successTimer * 100.0f) / TRACK_timer).ToString("0.00", strFormat));
            PhotonNetwork.SendAllOutgoingCommands();
        }

        foreach (var i in interfaces)
        {
            //if (i.TRACK_target.gameObject != null) Destroy(i.TRACK_target.gameObject);
            i.TRACK_AutomodeDisplay.gameObject.SetActive(true);
            i.TRACK_target.Restore();
        }

        TRACK_TASK_active = false;
        //TRACK_missedTarget = 0;
        TRACK_timer = 0.0f;
        TRACK_successTimer = 0.0f;
    }

    [PunRPC]
    private void TRACK_newdDirection(float dirX, float dirY)
    {
        float posX = 0.0f; float x = (dirX + 1.0f) / 2.0f; // float x = Random.Range(0.0f, 1.0f);
        float posY = 0.0f; float y = (dirY + 1.0f) / 2.0f; // float y = Random.Range(0.0f, 1.0f);
        foreach (var i in interfaces)
        {
            Bounds b = i.TRACK_targetContainer.bounds;
            posX = i.TRACK_target.inInnerZone() ? x * (b.max.x - b.min.x) + b.min.x : i.TRACK_target.transform.position.x;
            posY = i.TRACK_target.inInnerZone() ? y * (b.max.y - b.min.y) + b.min.y : i.TRACK_target.transform.position.y;
            i.TRACK_target.Move(new Vector3(posX, posY,  i.TRACK_target.transform.position.z), new Vector3(dirX, dirY, 0), TRACK_speed);
        }
    }

    [PunRPC]
    private void TRACK_Move(float x, float y)
    {
        foreach (var i in interfaces)
        {
            i.TRACK_target.Move(x,y);
        }
    }
    #endregion

    #region RESMAN
    private void UpdateRESMAN()
    {
        // Time
        if (RESMAN_TASK_active && RESMAN_timer >= RESMAN_duration)
        {
            photonView.RPC("RESMAN_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        else { RESMAN_timer += Time.deltaTime; }
        
        // Optimal range
        if (RESMAN_objective - 500 < RESMAN_tanks[(int) RESMAN_Tank.A]
            && RESMAN_tanks[(int) RESMAN_Tank.A] < RESMAN_objective + 500
            && RESMAN_objective - 500 < RESMAN_tanks[(int) RESMAN_Tank.B]
            && RESMAN_tanks[(int) RESMAN_Tank.B] < RESMAN_objective + 500)
        {
            RESMAN_optimalTimer += Time.deltaTime;
        }

        // Tanks
        float amout = Time.deltaTime;
        if (RESMAN_TASK_active) amout *= RESMAN_speedFactor;
        RESMAN_comsumeFuel(amout);
        
        // Pumps
        for (int p = 0; p < 8; p++)
        {
            RESMAN_pumpTimers[p] += Time.deltaTime;
            
            // Fuel flow
            if (RESMAN_pumpStates[p] == RESMAN_PumpState.Active)
            {
                RESMAN_Pump pump = (RESMAN_Pump) p;
                switch (pump)
                {
                    case RESMAN_Pump.AToB:
                        RESMAN_pumpFuel(RESMAN_Tank.A, RESMAN_Tank.B, RESMAN_Pump.AToB);
                        break;
                    case RESMAN_Pump.BToA:
                        RESMAN_pumpFuel(RESMAN_Tank.B, RESMAN_Tank.A, RESMAN_Pump.BToA);
                        break;
                    case RESMAN_Pump.CToA:
                        RESMAN_pumpFuel(RESMAN_Tank.C, RESMAN_Tank.A, RESMAN_Pump.CToA);
                        break;
                    case RESMAN_Pump.DToA:
                        RESMAN_pumpFuel(RESMAN_Tank.D, RESMAN_Tank.A, RESMAN_Pump.DToA);
                        break;
                    case RESMAN_Pump.DToC:
                        RESMAN_pumpFuel(RESMAN_Tank.D, RESMAN_Tank.C, RESMAN_Pump.DToC);
                        break;
                    case RESMAN_Pump.EToB:
                        RESMAN_pumpFuel(RESMAN_Tank.E, RESMAN_Tank.B, RESMAN_Pump.EToB);
                        break;
                    case RESMAN_Pump.FToB:
                        RESMAN_pumpFuel(RESMAN_Tank.F, RESMAN_Tank.B, RESMAN_Pump.FToB);
                        break;
                    case RESMAN_Pump.FToE:
                        RESMAN_pumpFuel(RESMAN_Tank.F, RESMAN_Tank.E, RESMAN_Pump.FToE);
                        break;
                }
            }

            // Failure - should we log these events ? change probability law for something else (eg. exponetial law)
            if (Random.Range(0.0f, 100.0f) < RESMAN_pumpFailChance && elapsedTime % 1.0f < 0.01f && RESMAN_pumpStates[p] == RESMAN_PumpState.Active) // RESMAN_pumpTimers[p] > RESMAN_pumpTimeLimit
            {
                photonView.RPC("RESMAN_changePump", RpcTarget.All, p, RESMAN_PumpState.Failed);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            else if (RESMAN_pumpTimers[p] > 2.0f && RESMAN_pumpStates[p] == RESMAN_PumpState.Failed)
            {
                photonView.RPC("RESMAN_changePump", RpcTarget.All, p, RESMAN_PumpState.Inactive);
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }

        // Periodic update on tanks and pumps status
        if (RESMAN_timer % 5.0f < 0.01f && photonView.IsMine) // maybe rework the periodicity
        {
            string status = RESMAN_getStatus();
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.RESMAN, "[]", "Update", true, status);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }
    
    [PunRPC]
    public void RESMAN_Start()
    {
        if (photonView.IsMine && !RESMAN_TASK_active)
        {
            string status = RESMAN_getStatus();
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.RESMAN, "[]", "Beginning of task", true, status);
            PhotonNetwork.SendAllOutgoingCommands();
        }

        RESMAN_TASK_active = true;
        RESMAN_timer = 0.0f;
        //RESMAN_optimalTimer = 0.0f;
        //RESMAN_ResetPumps();
        
    }

    [PunRPC]
    public void RESMAN_Reset()
    {
        if (photonView.IsMine)
        {
            string status = RESMAN_getStatus();
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.RESMAN, "[]", "End of task", true, status);
            PhotonNetwork.SendAllOutgoingCommands();
        }

        RESMAN_TASK_active = false;
        RESMAN_timer = 0.0f;
        //RESMAN_optimalTimer = 0.0f;
        //RESMAN_ResetPumps();
    }

    private void RESMAN_ResetPumps()
    {
        for (int i = 0; i < 8; i ++) { RESMAN_changePump(i, RESMAN_PumpState.Inactive); }
    }

    [PunRPC]
    private void RESMAN_changePump(int pump, RESMAN_PumpState state)
    {
        RESMAN_pumpStates[pump] = state;
        RESMAN_pumpTimers[pump] = 0.0f;
        foreach (var i in interfaces)
        {
            if (state == RESMAN_PumpState.Active) i.RESMAN_PumpButtons[pump].Activate();
            if (state == RESMAN_PumpState.Inactive) i.RESMAN_PumpButtons[pump].Restore();
            if (state == RESMAN_PumpState.Failed) i.RESMAN_PumpButtons[pump].Fail();
        }
    }

    private void RESMAN_pumpFuel(RESMAN_Tank from, RESMAN_Tank dest, RESMAN_Pump pump)
    {
        float amout = Time.deltaTime * RESMAN_PumpRates[pump];
        if (RESMAN_TASK_active) amout *= RESMAN_speedFactor;
        
        // From
        if (from != RESMAN_Tank.D && from != RESMAN_Tank.F)
        {
            if (RESMAN_tanks[(int) from] < amout)
            {
                amout = RESMAN_tanks[(int) from];
                RESMAN_tanks[(int) from] = 0.0f;
                photonView.RPC("RESMAN_changePump", RpcTarget.All, (int) pump, RESMAN_PumpState.Inactive);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            else RESMAN_tanks[(int) from] -= amout;
        }
        
        // Dest
        if (RESMAN_tanks[(int) dest] + amout > RESMAN_capacity[(int) dest])
        {
            RESMAN_tanks[(int) dest] = RESMAN_capacity[(int) dest];
            photonView.RPC("RESMAN_changePump", RpcTarget.All, (int) pump, RESMAN_PumpState.Inactive);
            PhotonNetwork.SendAllOutgoingCommands();
        }
        else RESMAN_tanks[(int) dest] += amout;
    }

    /// <summary>Comsume fuel (amout * consumption). </summary>
    private void RESMAN_comsumeFuel(float deltaTime)
    {
        // Tank A
        if (RESMAN_tanks[(int) RESMAN_Tank.A] > deltaTime * RESMAN_tankAConsumption)
        {
            RESMAN_tanks[(int) RESMAN_Tank.A] -= deltaTime * RESMAN_tankAConsumption;
        }
        else RESMAN_tanks[(int) RESMAN_Tank.A] = 0.0f;
        
        // Tank B
        if (RESMAN_tanks[(int) RESMAN_Tank.B] > deltaTime * RESMAN_tankBConsumption)
        {
            RESMAN_tanks[(int) RESMAN_Tank.B] -= deltaTime * RESMAN_tankBConsumption;
        }
        else RESMAN_tanks[(int) RESMAN_Tank.B] = 0.0f;
    }

    private string RESMAN_getStatus()
    {
        string tanks_status = "", pumps_status = "", timers = "";
            
        // Tanks status 
        for (int t = 0; t < RESMAN_tanks.Length; t++)
        {
            if (t == (int) RESMAN_Tank.D || t == (int) RESMAN_Tank.F) continue;
            tanks_status += RESMAN_tanks[t].ToString("0", strFormat) + ',';
        }

        // Pumps status
        for (int p = 0; p < 8; p++)
        {
            switch(RESMAN_pumpStates[p])
            {
                case RESMAN_PumpState.Active:
                    pumps_status += "Active: " + RESMAN_PumpRates[(RESMAN_Pump) p].ToString(strFormat);
                    break;
                case RESMAN_PumpState.Inactive:
                    pumps_status += "Inactive";
                    break;
                case RESMAN_PumpState.Failed:
                    pumps_status += "Failed";
                    break;
            }
            pumps_status += ','; 
        }

        // Time measures
        timers += RESMAN_optimalTimer.ToString("0.00", strFormat) + ',' + ((100 * RESMAN_optimalTimer) / elapsedTime).ToString("0.00", strFormat);

        return tanks_status + pumps_status + timers;
    }
    #endregion

    #region WORKLOAD
    [PunRPC]
    public void WrkLd_Start()
    {
        started = false;
        foreach (var i in interfaces)
        {
            //i.Workload.Activate();
            return;
        }
    }
    #endregion

    [ContextMenu("DEBUG")]
    public void DEBUG()
    {
        print("DEBUG");

        /*
        bool[] dir = {true, true, true, true};
        photonView.RPC("SYSMON_Start", RpcTarget.All, dir);
        PhotonNetwork.SendAllOutgoingCommands();
        */
        /*
        photonView.RPC ("COMM_Start", RpcTarget.All, 40) ;
        PhotonNetwork.SendAllOutgoingCommands () ;
        */

        //TRACK_Start(12,15);

        //COMM_LoadAudioResources();
    }
}