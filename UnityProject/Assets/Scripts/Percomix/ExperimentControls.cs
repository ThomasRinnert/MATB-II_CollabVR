using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;

public class ExperimentControls : MonoBehaviourPun, IPunObservable
{
    [Header("Links")]
    [SerializeField] public TASKPlanner planner;
    [SerializeField] public AggregativeUserBoard aggregBoard;
    [SerializeField] public List<DistributiveUserBoard> selfStatus = new List<DistributiveUserBoard>();

    [Header("Parameters")]
    [SerializeField] public Scenario scenario;
    [SerializeField] public string leftplayer;
    [SerializeField] public string rightplayer;
    [SerializeField] public Dictionary<MATBIISystem.MATBII_TASK, string> taskBinding;

    private bool aggreg = false;
    private bool distrib = false;
    private bool self = false;
    private bool divided = false;
    private int numplayers = 0;

    private bool SYSMON = false;
    private bool COMM = false;
    private bool TRACK = false;
    private bool RESMAN = false;

    private Coroutine SYSMON_coroutine = null;
    private Coroutine COMM_coroutine = null;
    private Coroutine TRACK_coroutine = null;
    private Coroutine RESMAN_coroutine = null;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(leftplayer);
            stream.SendNext(rightplayer);
            stream.SendNext(aggreg);
            stream.SendNext(distrib);
            stream.SendNext(self);
            stream.SendNext(divided);
            stream.SendNext(SYSMON);
            stream.SendNext(COMM);
            stream.SendNext(TRACK);
            stream.SendNext(RESMAN);
        }
        else
        {
            var lp = (string) stream.ReceiveNext(); leftplayer = lp; 
            var rp = (string) stream.ReceiveNext(); rightplayer = rp; 
            var _aggreg = (bool) stream.ReceiveNext();
            if (aggreg != _aggreg)
            {
                generateAggregativeBoards();
                activateAggregative(_aggreg);
            }
            var _distrib = (bool) stream.ReceiveNext();
            if (distrib != _distrib)
            {
                activateDistributive(_distrib);
            }
            var _self = (bool) stream.ReceiveNext();
            if (self != _self)
            {
                activateSelfStatus(_self);
            }
            var _divided = (bool) stream.ReceiveNext();
            if (divided != _divided && _divided)
            {
                divideTasks();
            }
            SYSMON = (bool) stream.ReceiveNext();
            COMM = (bool) stream.ReceiveNext();
            TRACK = (bool) stream.ReceiveNext();
            RESMAN = (bool) stream.ReceiveNext();
        }
    }

    private void Start()
    {
        GenerateAggregativeBoards();
    }

    private void Update()
    { 
        if (numplayers != GameManager.Instance.players.Count)
        {
            generateAggregativeBoards();
            activateAggregative(aggreg);
            activateDistributive(distrib); 
            activateSelfStatus(self);
            if(divided) divideTasks();
            numplayers = GameManager.Instance.players.Count;
        }
    }
    
    public void SetLeftPlayer(string player)
    {
        this.photonView.RPC("setLeftPlayer", RpcTarget.All, player);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void setLeftPlayer(string player) { leftplayer = player; }
    
    public void SetRightPlayer(string player)
    {
        this.photonView.RPC("setRightPlayer", RpcTarget.All, player);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void setRightPlayer(string player) { rightplayer = player; }
    
    public void Bind(MATBIISystem.MATBII_TASK task, string player)
    {
        this.photonView.RPC("bind", RpcTarget.All, task, player);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void bind(MATBIISystem.MATBII_TASK task, string player) { taskBinding[task] = player; }

    public void ClearBinding()
    {
        this.photonView.RPC("clearBinding", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void clearBinding() { taskBinding.Clear(); }

    public void GenerateAggregativeBoards()
    {
        this.photonView.RPC("generateAggregativeBoards", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void generateAggregativeBoards()
    {
        //aggregBoard.Generate();

        foreach (var player in GameManager.Instance.players)
        {
            var board = player.GetComponentInChildren<AggregativeUserBoard>(true);
            board.Generate();
        }
    }

    public void ActivateAggregative(bool active)
    {
        this.photonView.RPC("activateAggregative", RpcTarget.All, active);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void activateAggregative(bool active)
    {
        //aggregBoard.Activate(active);
        
        foreach (var player in GameManager.Instance.players)
        {
            var board = player.GetComponentInChildren<AggregativeUserBoard>(true);
            board.Activate(active);
        }
        
        aggreg = active;
    }

    public void ActivateDistributive(bool active)
    {
        this.photonView.RPC("activateDistributive", RpcTarget.All, active);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void activateDistributive(bool active)
    {
        foreach (var player in GameManager.Instance.players)
        {
            var board = player.GetComponentInChildren<DistributiveUserBoard>(true);
            board.Activate(active);
        }
        distrib = active;
    }

    public void ActivateSelfStatus(bool active)
    {
        this.photonView.RPC("activateSelfStatus", RpcTarget.All, active);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void activateSelfStatus(bool active)
    {
        foreach (var board in selfStatus)
        {
            board.Activate(active, true);
        }
        self = active;
    }

    public void DivideTasks()
    {
        this.photonView.RPC("divideTasks", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void divideTasks()
    {
        foreach (var player in GameManager.Instance.players)
        {
            //aggreg
            var aggregBoard = player.GetComponentInChildren<AggregativeUserBoard>(true);
            aggregBoard.DivideTasks(leftplayer, rightplayer);
            
            //distrib
            var board = player.GetComponentInChildren<DistributiveUserBoard>(true);
            if (player.NickName == leftplayer) { board.GetComponentInChildren<ActionHistory>().divideTasks(true); }
            if (player.NickName == rightplayer) { board.GetComponentInChildren<ActionHistory>().divideTasks(false); }
        }

        //self reflection
        foreach (var board in selfStatus)
        {
            if (board.player_manager == null) board.Init();
            if (board.player_manager.NickName == leftplayer)
                { board.GetComponentInChildren<ActionHistory>(true).divideTasks(true); }
            if (board.player_manager.NickName == rightplayer)
                { board.GetComponentInChildren<ActionHistory>(true).divideTasks(false); }
        }
        divided = true;
    }

    public void ClearTaskDivision()
    {
        this.photonView.RPC("clearTaskDivision", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void clearTaskDivision()
    {
        aggregBoard.clearTaskDivision();
        foreach (var player in GameManager.Instance.players)
        {
            player.GetComponentInChildren<ActionHistory>(true).clearTaskDivision();
        }
        foreach (var board in selfStatus)
        {
            if (board.player_manager == null) board.Init();
            board.GetComponentInChildren<ActionHistory>(true).clearTaskDivision();
        }
        
        divided = false;
    }
    
    public void StartExperiment()
    {
        this.photonView.RPC("startExperiment", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void startExperiment()
    {
        ALL_Reset();
        planner.planningIndex = scenario;
        planner.planning = planner.scenarii[scenario];
        MATBIISystem.Instance.training = false;
        planner.StartTasks();
    }

    public void SYSMON_Start(bool[] Sysmon_parameters)
    {
        if (SYSMON) StopCoroutine(SYSMON_coroutine);
        else SYSMON = true;
        SYSMON_coroutine = StartCoroutine(SYSMON_Coroutine(Sysmon_parameters));
    }
    public IEnumerator SYSMON_Coroutine(bool[] Sysmon_parameters)
    {
        bool[] parameters = new bool[6]; System.Array.Copy(Sysmon_parameters, parameters, Sysmon_parameters.Length);
        while(SYSMON == true)
        {
            bool[] directions = {Random.Range(float.MinValue, float.MaxValue) > 0.0f,
                Random.Range(float.MinValue, float.MaxValue) > 0.0f,
                Random.Range(float.MinValue, float.MaxValue) > 0.0f,
                Random.Range(float.MinValue, float.MaxValue) > 0.0f};
            
            MATBIISystem.Instance.photonView.RPC("SYSMON_Start", RpcTarget.All, parameters, directions);
            PhotonNetwork.SendAllOutgoingCommands();
            MATBIISystem.Instance.started = true;

            parameters[0] = Random.Range(float.MinValue, float.MaxValue) > 0.0f;
            parameters[1] = Random.Range(float.MinValue, float.MaxValue) > 0.0f;
            parameters[2] = Random.Range(float.MinValue, float.MaxValue) > 0.0f;
            parameters[3] = Random.Range(float.MinValue, float.MaxValue) > 0.0f;
            parameters[4] = Random.Range(float.MinValue, float.MaxValue) > 0.0f;
            parameters[5] = Random.Range(float.MinValue, float.MaxValue) > 0.0f;

            yield return new WaitForSeconds(MATBIISystem.Instance.SYSMON_timeLimit * 1.1f);
        }
    }
    public void SYSMON_Stop()
    {
        SYSMON = false;
        MATBIISystem.Instance.photonView.RPC ("SYSMON_Reset", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    public void COMM_Start()
    {
        if (COMM) StopCoroutine(COMM_coroutine);
        else COMM = true;
        COMM_coroutine = StartCoroutine(COMM_Coroutine());
    }
    public IEnumerator COMM_Coroutine()
    {
        while(COMM == true)
        {
            MATBIISystem.Instance.photonView.RPC("COMM_Start", RpcTarget.All, Random.Range(0, MATBIISystem.Instance.COMM_transmissions.Count));
            PhotonNetwork.SendAllOutgoingCommands();
            MATBIISystem.Instance.started = true;
            yield return new WaitForSeconds(MATBIISystem.Instance.COMM_timeLimit * 1.1f);
        }
    }
    public void COMM_Stop()
    {
        COMM = false;
        MATBIISystem.Instance.photonView.RPC ("COMM_Reset", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    public void TRACK_Start()
    {
        if (TRACK) StopCoroutine(TRACK_coroutine);
        else TRACK = true;
        TRACK_coroutine = StartCoroutine(TRACK_Coroutine());
    }
    public IEnumerator TRACK_Coroutine()
    {
        while(TRACK == true)
        {
            MATBIISystem.Instance.photonView.RPC("TRACK_Start", RpcTarget.All, Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            PhotonNetwork.SendAllOutgoingCommands();
            MATBIISystem.Instance.started = true;
            yield return new WaitForSeconds(MATBIISystem.Instance.TRACK_duration * 1.1f);
        }
    }
    public void TRACK_Stop()
    {
        TRACK = false;
        MATBIISystem.Instance.photonView.RPC ("TRACK_Reset", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    public void RESMAN_Start()
    {
        if (RESMAN) StopCoroutine(RESMAN_coroutine);
        else RESMAN = true;
        RESMAN_coroutine = StartCoroutine(RESMAN_Coroutine());
    }
    public IEnumerator RESMAN_Coroutine()
    {
        while(RESMAN == true)
        {
            MATBIISystem.Instance.photonView.RPC("RESMAN_Start", RpcTarget.All);
            PhotonNetwork.SendAllOutgoingCommands();
            MATBIISystem.Instance.started = true;
            yield return new WaitForSeconds(MATBIISystem.Instance.RESMAN_duration * 1.1f);
        }
    }
    public void RESMAN_Stop()
    {
        RESMAN = false;
        MATBIISystem.Instance.photonView.RPC ("RESMAN_Reset", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    public void ALL_Start()
    {
        SYSMON_Start(new bool[]{true, true, true, true, true, true});
        COMM_Start();
        TRACK_Start();
        RESMAN_Start();
    }
    public void ALL_Stop()
    {
        SYSMON_Stop();
        COMM_Stop();
        TRACK_Stop();
        RESMAN_Stop();
        MATBIISystem.Instance.started = false;
    }
    public void ALL_Reset()
    {
        ALL_Stop();
        MATBIISystem.Instance.photonView.RPC ("RESMAN_Refuel", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ExperimentControls))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExperimentControls script = (ExperimentControls)target;
        if(GUILayout.Button("Generate Aggregative Boards"))
        {
            script.GenerateAggregativeBoards();
        }
        if(GUILayout.Button("Activate Aggregative Boards"))
        {
            script.ActivateAggregative(true);
        }
        if(GUILayout.Button("Deactivate Aggregative Boards"))
        {
            script.ActivateAggregative(false);
        }
        if(GUILayout.Button("Activate Distributive Boards"))
        {
            script.ActivateDistributive(true);
        }
        if(GUILayout.Button("Deactivate Distributive Boards"))
        {
            script.ActivateDistributive(false);
        }
        if(GUILayout.Button("Activate Feedback of self status"))
        {
            script.ActivateSelfStatus(true);
        }
        if(GUILayout.Button("Deactivate Feedback of self status"))
        {
            script.ActivateSelfStatus(false);
        }

        //Left / Right division
        for (int p = 0; p < GameManager.Instance.players.Count; p++)
        {
            if (GameManager.Instance.players[p].NickName.Contains("XP")) continue;

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Give left battery to " + GameManager.Instance.players[p].NickName))
            {
                script.setLeftPlayer(GameManager.Instance.players[p].NickName);
            }
            if(GUILayout.Button("Give right battery to " + GameManager.Instance.players[p].NickName))
            {
                script.setRightPlayer(GameManager.Instance.players[p].NickName);
            }
            GUILayout.EndHorizontal();
        }

        // Task specific division
        /*
        for (int p = 0; p < PhotonNetwork.PlayerList.Length; p++)
        {
            if (PhotonNetwork.PlayerList[p].NickName.Contains("XP")) continue;
            
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Bind SYSMON to " + PhotonNetwork.PlayerList[p].NickName))
            {
                script.Bind(MATBIISystem.MATBII_TASK.SYSMON, PhotonNetwork.PlayerList[p].NickName);
            }
            if(GUILayout.Button("Bind COMM to " + PhotonNetwork.PlayerList[p].NickName))
            {
                script.Bind(MATBIISystem.MATBII_TASK.COMM, PhotonNetwork.PlayerList[p].NickName);
            }
            if(GUILayout.Button("Bind TRACK to " + PhotonNetwork.PlayerList[p].NickName))
            {
                script.Bind(MATBIISystem.MATBII_TASK.TRACK, PhotonNetwork.PlayerList[p].NickName);
            }
            if(GUILayout.Button("Bind RESMAN to " + PhotonNetwork.PlayerList[p].NickName))
            {
                script.Bind(MATBIISystem.MATBII_TASK.RESMAN, PhotonNetwork.PlayerList[p].NickName);
            }
            GUILayout.EndHorizontal();
        }
        */
        
        if(GUILayout.Button("Divide tasks"))
        {
            script.DivideTasks();
        }
        if(GUILayout.Button("Clear task division"))
        {
            script.ClearTaskDivision();
        }
        if(GUILayout.Button("Start Experiment"))
        {
            script.StartExperiment();
        }

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("TRAIN: SYSMON_Start"))
        {
            script.SYSMON_Start(new bool[]{true, true, true, true, true, true});
        }
        if(GUILayout.Button("TRAIN: SYSMON_Stop"))
        {
            script.SYSMON_Stop();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("TRAIN: COMM_Start"))
        {
            script.COMM_Start();
        }
        if(GUILayout.Button("TRAIN: COMM_Stop"))
        {
            script.COMM_Stop();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("TRAIN: TRACK_Start"))
        {
            script.TRACK_Start();
        }
        if(GUILayout.Button("TRAIN: TRACK_Stop"))
        {
            script.TRACK_Stop();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("TRAIN: RESMAN_Start"))
        {
            script.RESMAN_Start();
        }
        if(GUILayout.Button("TRAIN: RESMAN_Stop"))
        {
            script.RESMAN_Stop();
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("TRAIN: ALL_Start"))
        {
            script.ALL_Start();
        }
        if(GUILayout.Button("TRAIN: ALL_Stop"))
        {
            script.ALL_Stop();
        }
        if(GUILayout.Button("TRAIN: ALL_Reset"))
        {
            script.ALL_Reset();
        }
        GUILayout.EndHorizontal();
    }
}
#endif
