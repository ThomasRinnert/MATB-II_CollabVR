using System.Collections;
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
    private bool[] SYSMON_request = {false,false,false,false,false,false};

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
    public bool isSYSMON_Scales_active(int index) { return (index < 4) ? SYSMON_Scales_active[index] : false; }
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
    public string[] COMM_radioNames = {"NAV1", "NAV2", "COM1", "COM2"};
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
    [SerializeField]
    private AudioTransmission COMM_onGoingTransmission = null;
    public AudioTransmission getCOMM_onGoingTransmission(){return COMM_onGoingTransmission;}
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
    private double TRACK_iterationScore = 0;

    private string TRACK_authors = "";
    private double [] TRACK_authorContrib = {0.0, 0.0};

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
    [SerializeField] [Tooltip("Factor applied to flow/consumption rates")] [Range(1.0f, 5.0f)]
    public float RESMAN_speedFactor = 2.0f;
    //[SerializeField] [Tooltip("Mean time before pump failure")] [Range(0.0f, 20.0f)]
    //public float RESMAN_pumpFail = 1.0f;
    [SerializeField] [Tooltip("Min time before pump failure")] [Range(0.0f, 20.0f)]
    public float RESMAN_pumpFailMin = 5.0f;
    [SerializeField] [Tooltip("Max time before pump failure")] [Range(0.0f, 20.0f)]
    public float RESMAN_pumpFailMax = 20.0f;
    [SerializeField] [Tooltip("Time to restore a pump failure")] [Range(0.0f, 10.0f)]
    public float RESMAN_pumpTimeLimit = 2.0f;

    //[SerializeField] [Tooltip("Fuel consumption of tank A")] [Range(1.0f, 15.0f)]
    private float RESMAN_tankAConsumption = 12.0f; // NASA default: 800/min = 13.33f
    //[SerializeField] [Tooltip("Fuel consumption of tank B")] [Range(1.0f, 15.0f)]
    private float RESMAN_tankBConsumption = 12.0f; // NASA default: 800/min = 13.33f
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
    private float[] RESMAN_pumpFails = {0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
    #endregion

    [SerializeField] public List<MATBIIInterface> interfaces;
    public int sessionID = 0;
    public string logSessionFolder;
    public System.Globalization.CultureInfo strFormat = System.Globalization.CultureInfo.InvariantCulture;
    public double elapsedTime;
    public bool started = false;
    public bool training = true;
    private RandomGenerator RNG;
    public TASKPlanner planner;

    // Scoring: 25% for each task, earned by completing perfectly all iterations of a task
    private double SYSMON_score = 0; public double getSYSMON_score() { return SYSMON_score; }
    public double SYSMON_scoreIncrement = 1.0;
    private double COMM_score = 0; public double getCOMM_score() { return COMM_score; }
    public double COMM_scoreIncrement = 1.0;
    private double TRACK_score = 0; public double getTRACK_score() { return TRACK_score; }
    public double TRACK_scoreIncrement = 1.0;
    private double RESMAN_score = 0; public double getRESMAN_score() { return RESMAN_score; }
    //public double RESMAN_scoreIncrement = 1.0;
    public double MATBII_score { get => SYSMON_score + COMM_score + TRACK_score + RESMAN_score; }

    // Photon Events code
    public enum PhotonEventCodes : byte
    {
        SYSMON_SwitchPressed,
        SYSMON_ScalePressed,
        COMM_RadioChanged,
        COMM_Validation,
        TRACK_TargetMove,
        RESMAN_PumpChanged,
        WRS_Validation,
    }

    // IPunObservable callback
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(started);
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

            stream.SendNext(SYSMON_score);
            stream.SendNext(COMM_score);
            stream.SendNext(TRACK_score);
            stream.SendNext(RESMAN_score);
        }
        else
        {
            this.started                   = (bool)   stream.ReceiveNext();
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

            this.SYSMON_score         = (double) stream.ReceiveNext();
            this.COMM_score           = (double) stream.ReceiveNext();
            this.TRACK_score          = (double) stream.ReceiveNext();
            this.RESMAN_score         = (double) stream.ReceiveNext();
        }
    }

    // IOnEventCallback
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        
        if (eventCode == (byte) PhotonEventCodes.SYSMON_SwitchPressed)
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

        if (eventCode == (byte) PhotonEventCodes.SYSMON_ScalePressed)
        {
            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";
            int index = (int)data[1];

            if (!SYSMON_Scales_active[index])
            { 
                // photonView.RPC("SYSMON_scale_Reset", RpcTarget.All, index);
                // PhotonNetwork.SendAllOutgoingCommands();
                return;
            }
            
            if (SYSMON_Scales_active[index] && photonView.IsMine)
            {
                photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, author, "Scale", true, SYSMON_Scales_timers[index].ToString("0.0000", strFormat) + "(" + SYSMON_Scales_criticalTimers[index].ToString("0.0000", strFormat) + ")");
                photonView.RPC("SYSMON_scale_Reset", RpcTarget.All, index);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            return;
        }
        
        if (eventCode == (byte) PhotonEventCodes.COMM_RadioChanged)
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

                //if (COMM_onGoingTransmission.radio == r && COMM_onGoingTransmission.frequency == frequency && COMM_onGoingTransmission.isNASA504) ?
            }
            return;
        }

        if (eventCode == (byte) PhotonEventCodes.COMM_Validation)
        {
            if (!COMM_TASK_active) return;

            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";

            int r = (int) COMM_onGoingTransmission.radio;
            float freq = COMM_onGoingTransmission.frequency;
            bool success = false; string type = "";
            string modifiedByUser = author + " [ "; string modifiedRadios = "[ "; string modifiedFrequencies = "[ ";
            for (int i = 0; i < COMM_latestModif.Length; i++)
            {
                if (COMM_latestModif[i] != "none")
                {
                    modifiedByUser += COMM_latestModif[i].ToString(strFormat) + " ";
                    modifiedRadios += COMM_radioNames[i].ToString(strFormat) + " ";
                    modifiedFrequencies += COMM_frequencies[i].ToString("000.000", strFormat) + " ";
                }
            }
            modifiedByUser += "]"; modifiedRadios += "]"; modifiedFrequencies += "]";

            // COMM Transmission was not adressed to users ("NASA 504")
            if (!COMM_onGoingTransmission.isNASA504)
            {
                type = "Radio (Not adressed to NASA 504)"; success = !COMM_responded;
            }

            // Failure
            else if (COMM_responseTimer + COMM_completionTimer > COMM_timeLimit // Time limit
                  //|| COMM_latestModif[r] == "none"                            // No one changed the target radio
                    || COMM_frequencies[r] != freq)                             // Wrong channel on target radio
            {
                type = "Radio"; success = false;
            }

            // Success
            else if (COMM_frequencies[r] == freq)
            {
                type = "Radio"; success = true;
            }

            if (!photonView.IsMine) return;

            //if (success) COMM_score += COMM_scoreIncrement * ((COMM_responseTimer + COMM_completionTimer) / COMM_timeLimit);
            if (success) COMM_score += COMM_scoreIncrement;
            else
            {
                string s1 = freq.ToString("000.000", strFormat);
                string s2 = COMM_frequencies[r] .ToString("000.000", strFormat);
                int count = -2;
                for (int i = 0; i < s2.Length; i++)
                {
                    if (s1[i] == s2[i]) count ++;
                }
                COMM_score += COMM_scoreIncrement * (((double)count) / 5.0); // with the 7 digits, only five can change, all frequencies start with a 1 and have a '.'
            }
            
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.COMM, modifiedByUser, type, success,
                COMM_radioNames[r] + ", " + 
                freq.ToString("0.000", strFormat) + ", " + 
                modifiedRadios + ", " + 
                modifiedFrequencies + ", " + 
                COMM_responseTimer.ToString("0.0000", strFormat) + ", " + 
                COMM_completionTimer.ToString("0.0000", strFormat));
            photonView.RPC("COMM_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
            return;
        }

        if (eventCode == (byte) PhotonEventCodes.WRS_Validation)
        {
            if (!COMM_TASK_active) return;

            object[] data = (object[])photonEvent.CustomData;
            string author = (data != null) ? (string)data[0] : "[ ]";

            //mental
            //physical

            int r = (int) COMM_onGoingTransmission.radio;
            float freq = COMM_onGoingTransmission.frequency;
            bool success = false; string type = "";
            string modifiedByUser = author + " [ "; string modifiedRadios = "[ "; string modifiedFrequencies = "[ ";
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

            // COMM Transmission was not adressed to users ("NASA 504")
            if (!COMM_onGoingTransmission.isNASA504)
            {
                type = "Radio (Not adressed to NASA 504)"; success = !COMM_responded;
            }

            // Failure
            else if (COMM_responseTimer + COMM_completionTimer > COMM_timeLimit // Time limit
                    || COMM_latestModif[r] == "none"                            // No one changed the target radio
                    || COMM_frequencies[r] != freq)                             // Wrong channel on target radio
            {
                type = "Radio"; success = false;
            }

            // Success
            else if (COMM_frequencies[r] == freq)
            {
                type = "Radio"; success = true;
            }

            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.COMM, modifiedByUser, type, success,
                COMM_radioNames[r] + ", " + 
                freq.ToString("0.000", strFormat) + ", " + 
                modifiedRadios + ", " + 
                modifiedFrequencies + ", " + 
                COMM_responseTimer.ToString("0.0000", strFormat) + ", " + 
                COMM_completionTimer.ToString("0.0000", strFormat));
            photonView.RPC("COMM_Reset", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
            return;
        }

        if (eventCode == (byte) PhotonEventCodes.TRACK_TargetMove)
        {
            if (!TRACK_TASK_active) return;

            object[] data = (object[])photonEvent.CustomData;
            if (data == null) return;

            string author = (string)data[0];
            float x = (float)data[1];
            float y = (float)data[2];

            if (photonView.IsMine)
            {
                if (author.Contains("Unity")) TRACK_authorContrib[0] += 1;
                else TRACK_authorContrib[1] += 1;
                
                photonView.RPC("TRACK_Move", RpcTarget.All, x, y);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            return;
        }

        if (eventCode == (byte) PhotonEventCodes.RESMAN_PumpChanged)
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

        if (eventCode >= 200) { return; /* Photon internal events */ }

        Debug.LogWarning("Unhandled EventData from photon... event code: " + eventCode.ToString());
        return;
    }

    void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        RNG = GetComponent<RandomGenerator>();
        planner = GetComponent<TASKPlanner>();

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
                if (SYSMON_Scales_criticalTimers[s] + SYSMON_Scales_timers[s] > SYSMON_timeLimit)
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
    public void SYSMON_Start(bool[] tasks, bool[] direction, PhotonMessageInfo info)
    {
        SYSMON_TASK_active = true;

        if (tasks[0]) SYSMON_normallyON_Start();
        if (tasks[1]) SYSMON_normallyOFF_Start();
        for (int i = 0; i < 4; i++) { if (tasks[2+i]) SYSMON_scale_Start(i,direction[i]); }
        SYSMON_request = tasks;
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
            PhotonNetwork.SendAllOutgoingCommands();
        }
        if (SYSMON_NormallyON_active) SYSMON_normallyON_Reset();
        
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
        //SYSMON_NormallyON_timer = 0.0f;
        SYSMON_NormallyON_active = false;
        foreach (var i in interfaces)
        {
            i.SYSMON_normallyON_Switch.Restore();
        }

        SYSMON_UpdateStatus();
    }

    [PunRPC]
    public void SYSMON_normallyOFF_Start()
    {
        if (SYSMON_NormallyOFF_active && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Normally OFF Light", false, SYSMON_NormallyOFF_timer.ToString("0.0000", strFormat));
            PhotonNetwork.SendAllOutgoingCommands();
        }
        if (SYSMON_NormallyOFF_active) SYSMON_normallyOFF_Reset();
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
        //SYSMON_NormallyOFF_timer = 0.0f;
        SYSMON_NormallyOFF_active = false;
        foreach (var i in interfaces)
        {
            i.SYSMON_normallyOFF_Switch.Restore();
        }

        SYSMON_UpdateStatus();
    }

    [PunRPC]
    public void SYSMON_scale_Start(int scale, bool direction)
    {
        if (SYSMON_Scales_active[scale] && photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.SYSMON, "[ ]", "Scale", false, SYSMON_Scales_timers[scale].ToString("0.0000", strFormat) + "(" + SYSMON_Scales_criticalTimers[scale].ToString("0.0000", strFormat) + ")");
            PhotonNetwork.SendAllOutgoingCommands();
        }
        if (SYSMON_Scales_active[scale]) SYSMON_scale_Reset(scale);

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
        //SYSMON_Scales_timers[scale] = 0.0f;
        //SYSMON_Scales_criticalTimers[scale] = 0.0f;
        SYSMON_Scales_active[scale] = false;
        foreach (var i in interfaces)
        {
            i.SYSMON_Scales[scale].ResetPos();
        }

        SYSMON_UpdateStatus();
    }

    private void SYSMON_UpdateStatus()
    {
        bool old_value = SYSMON_TASK_active;
        SYSMON_TASK_active = SYSMON_NormallyON_active || SYSMON_NormallyOFF_active || SYSMON_Scales_active[0] || SYSMON_Scales_active[1] || SYSMON_Scales_active[2] || SYSMON_Scales_active[3];
        if (old_value && !SYSMON_TASK_active)
        {
            float factor = 0.0f; int task_quantity = 0;
            

            if (SYSMON_request[0])
            {
                factor += 1 - (SYSMON_NormallyON_timer / SYSMON_timeLimit);
                task_quantity += 1;
            }
            if (SYSMON_request[1])
            {
                factor += 1 - (SYSMON_NormallyOFF_timer / SYSMON_timeLimit);
                task_quantity += 1;
            }
            for (int i = 0; i < 4; i++)
            {
                if (SYSMON_request[2+i])
                {
                    factor += 1 - ((SYSMON_Scales_timers[i] + SYSMON_Scales_criticalTimers[i]) / SYSMON_timeLimit);
                    task_quantity += 1;
                }
            }
            SYSMON_score += (SYSMON_scoreIncrement) * (factor / task_quantity);
            SYSMON_NormallyON_timer = 0.0f;
            SYSMON_NormallyOFF_timer = 0.0f;
            for (int scale = 0; scale < 4; scale++)
            {
                SYSMON_Scales_timers[scale] = 0.0f;
                SYSMON_Scales_criticalTimers[scale] = 0.0f;
            }
        }
    }
    #endregion

    #region COMM
    private void UpdateCOMM()
    {
        if (!COMM_responded) { COMM_responseTimer += Time.deltaTime; }
        else { COMM_completionTimer += Time.deltaTime; }

        if (COMM_responseTimer + COMM_completionTimer > COMM_timeLimit)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent((byte) MATBIISystem.PhotonEventCodes.COMM_Validation, new object[] {"None"}, raiseEventOptions, SendOptions.SendReliable);
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

        foreach (var i in interfaces) { i.COMM_Validate.gameObject.SetActive(true); }
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

        foreach (var i in interfaces) { i.COMM_Validate.gameObject.SetActive(false); }

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
            TRACK_iterationScore = TRACK_successTimer / TRACK_duration; //Take distance into account
        }
        else
        {
            TRACK_score += TRACK_scoreIncrement * TRACK_iterationScore;
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
        TRACK_authors = "";
        TRACK_authorContrib[0] = 0.0;
        TRACK_authorContrib[1] = 0.0;

        if (photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.TRACK, "[ ]", "MANUAL MODE", true, "");
            PhotonNetwork.SendAllOutgoingCommands();
            StartCoroutine(TRACK_Coroutine());
        }
    }
    public IEnumerator TRACK_Coroutine()
    {
        while (TRACK_TASK_active)
        {
            photonView.RPC("TRACK_newDirection", RpcTarget.All, Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            PhotonNetwork.SendAllOutgoingCommands();
            yield return new WaitForSeconds(5.0f);
        }
    }

    [PunRPC]
    public void TRACK_Reset()
    {
        //Disable tracking tool
        //...

        TRACK_authors = "VR_Unity: " + ((100.0 * TRACK_authorContrib[0]) / (TRACK_authorContrib[0] + TRACK_authorContrib[1])).ToString("00.00", strFormat) + "% VR_U: " + ((100.0 * TRACK_authorContrib[1]) / (TRACK_authorContrib[0] + TRACK_authorContrib[1])).ToString("00.00", strFormat) + "%";

        if (photonView.IsMine)
        {
            photonView.RPC("LogEvent", RpcTarget.All, MATBII_TASK.TRACK, TRACK_authors, "MANUAL MODE", false, ((TRACK_successTimer * 100.0f) / TRACK_timer).ToString("0.00", strFormat));
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
        TRACK_authors = "";
        TRACK_authorContrib[0] = 0.0;
        TRACK_authorContrib[1] = 0.0;
    }

    [PunRPC]
    private void TRACK_newDirection(float dirX, float dirY)
    {
        // float posX = 0.0f; 
        // float posY = 0.0f;
        float x = (dirX + 1.0f) / 2.0f; // float x = Random.Range(0.0f, 1.0f);
        float y = (dirY + 1.0f) / 2.0f; // float y = Random.Range(0.0f, 1.0f);
        foreach (var i in interfaces)
        {
            // Bounds b = i.TRACK_targetContainer.bounds;
            // posX = i.TRACK_target.inInnerZone() ? x * (b.size.x) + b.min.x : i.TRACK_target.transform.position.x;
            // posY = i.TRACK_target.inInnerZone() ? y * (b.size.y) + b.min.y : i.TRACK_target.transform.position.y;
            // Vector3 position = new Vector3(posX, posY,  i.TRACK_target.transform.position.z);
            i.TRACK_target.Move(i.TRACK_target.transform.position, new Vector3(dirX, dirY, 0), TRACK_speed);
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
        && RESMAN_tanks[(int) RESMAN_Tank.A] < RESMAN_objective + 500)
        { RESMAN_optimalTimer += Time.deltaTime * 0.5f; }

        if (RESMAN_objective - 500 < RESMAN_tanks[(int) RESMAN_Tank.B]
        && RESMAN_tanks[(int) RESMAN_Tank.B] < RESMAN_objective + 500)
        { RESMAN_optimalTimer += Time.deltaTime * 0.5f; }

        RESMAN_score = (25.0 * RESMAN_optimalTimer) / planner.planning.duration; // / elapsedTime;

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

            // Failure - should we log these events ?
            if (RESMAN_pumpStates[p] == RESMAN_PumpState.Active && RESMAN_pumpTimers[p] > RESMAN_pumpFails[p])
            {
                photonView.RPC("RESMAN_changePump", RpcTarget.All, p, RESMAN_PumpState.Failed);
                PhotonNetwork.SendAllOutgoingCommands();
            }
            else if (RESMAN_pumpStates[p] == RESMAN_PumpState.Failed && RESMAN_pumpTimers[p] > RESMAN_pumpTimeLimit)
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

    [PunRPC]
    public void RESMAN_Refuel()
    {
        RESMAN_Reset();
        RESMAN_tanks = new float[]{2500.0f, 2500.0f, 1500.0f, float.MaxValue, 1500.0f, float.MaxValue};
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
        
        if (state == RESMAN_PumpState.Active)
        {
            /*
            Exponential distribution
                PDF:	lambda * e^{- lambda x}
                CDF:	1 - e^{- lambda x}}
                Mean:   1/ lambda = RESMAN_pumpFail
                => lifespan = - (ln (1 - CDF(x))) / lambda
            double u = RNG.MRG32k3a();
            double s = RESMAN_pumpFail;
            double l = RESMAN_TASK_active ? (s / RESMAN_speedFactor) : s;
            RESMAN_pumpFails[pump] = (float) (- System.Math.Log(1.0-u) * l);
            */

            RESMAN_pumpFails[pump] = Random.Range(RESMAN_pumpFailMin, RESMAN_pumpFailMax);
        }

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
        }
        started = true;
    }
    #endregion

    [ContextMenu("START")]
    public void StartBattery()
    {
        ResetBattery();
        SYSMON_scoreIncrement = 25.0 / ((float) planner.planning.SYSMON_Tasks.Count);
        COMM_scoreIncrement = 25.0 / ((float) planner.planning.COMM_Tasks.Count);
        TRACK_scoreIncrement = 25.0 / ((float) planner.planning.TRACK_Tasks.Count);
        //RESMAN_scoreIncrement = 25.0 / ((float) planner.planning.RESMAN_Tasks.Count);
        started = true;
    }

    [ContextMenu("RESET")]
    public void ResetBattery()
    {
        RESMAN_Refuel();
        elapsedTime = 0.0f;
        RESMAN_optimalTimer = 0.0f;
        SYSMON_score = 0;
        COMM_score = 0;
        TRACK_score = 0;
        RESMAN_score = 0;
    }

    [ContextMenu("DEBUG")]
    public void DEBUG()
    {
        print("DEBUG");

        //interfaces[0].COMM_audio.Play();

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