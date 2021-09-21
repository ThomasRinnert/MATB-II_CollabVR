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
    [SerializeField] public DistributiveUserBoard selfStatus;

    [Header("Parameters")]
    [SerializeField] public Scenario scenario;
    [SerializeField] public string leftplayer;
    [SerializeField] public string rightplayer;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(leftplayer);
            stream.SendNext(rightplayer);
        }
        else
        {
            leftplayer = (string) stream.ReceiveNext();
            rightplayer = (string) stream.ReceiveNext();
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

    public void GenerateAggregativeBoards()
    {
        this.photonView.RPC("generateAggregativeBoards", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void generateAggregativeBoards()
    {
        aggregBoard.Generate();
    }

    public void ActivateAggregative(bool active)
    {
        this.photonView.RPC("activateAggregative", RpcTarget.All, active);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void activateAggregative(bool active)
    {
        aggregBoard.Activate(active);
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
    }

    public void ActivateSelfStatus(bool active)
    {
        this.photonView.RPC("activateSelfStatus", RpcTarget.All, active);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void activateSelfStatus(bool active)
    {
        selfStatus.Activate(active);
    }

    public void DivideTasks()
    {
        this.photonView.RPC("divideTasks", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void divideTasks()
    {
        aggregBoard.DivideTasks(leftplayer, rightplayer);
        foreach (var player in GameManager.Instance.players)
        {
            var board = player.GetComponentInChildren<ActionHistory>(true);
            if (player.NickName == leftplayer) { board.divideTasks(true); }
            if (player.NickName == rightplayer) { board.divideTasks(false); }
        }
        if (selfStatus.player_manager.NickName == leftplayer) { selfStatus.GetComponentInChildren<ActionHistory>().divideTasks(true); }
        if (selfStatus.player_manager.NickName == rightplayer) { selfStatus.GetComponentInChildren<ActionHistory>().divideTasks(false); }
    }
    
    public void StartExperiment()
    {
        this.photonView.RPC("startExperiment", RpcTarget.All);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC] public void startExperiment()
    {
        planner.planning = planner.scenarii[scenario];
        planner.StartTasks();
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

        for (int p = 0; p < PhotonNetwork.PlayerList.Length; p++)
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Give left battery to " + PhotonNetwork.PlayerList[p].NickName))
            {
                script.setLeftPlayer(PhotonNetwork.PlayerList[p].NickName);
            }
            if(GUILayout.Button("Give right battery to " + PhotonNetwork.PlayerList[p].NickName))
            {
                script.setRightPlayer(PhotonNetwork.PlayerList[p].NickName);
            }
            GUILayout.EndHorizontal();
        }
        
        if(GUILayout.Button("Divide tasks"))
        {
            script.DivideTasks();
        }
        if(GUILayout.Button("Start Experiment"))
        {
            script.StartExperiment();
        }
    }
}
#endif
