using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum Scenario : int
{
    Training = 0,
    A_B_AB = 1,
    B_AB_A = 2,
    AB_A_B = 3,
    AB_B_A = 4,
    B_A_AB = 5,
    A_AB_B = 6,
    DemoCNRS = 7
}

public class TASKPlanner : MonoBehaviourPun, IPunObservable
{
    MATBIISystem mb2;

    public int SYSMON_index;
    public int COMM_index;
    public int TRACK_index;
    public int RESMAN_index;
    public int WrkLd_index;

    [Header("Scenario variants")]
    [SerializeField] public TASKPlanning Training;
    [SerializeField] public TASKPlanning A_B_AB;
    [SerializeField] public TASKPlanning B_AB_A;
    [SerializeField] public TASKPlanning AB_A_B;
    [SerializeField] public TASKPlanning AB_B_A;
    [SerializeField] public TASKPlanning B_A_AB;
    [SerializeField] public TASKPlanning A_AB_B;
    [SerializeField] public TASKPlanning DemoCNRS;

    public Dictionary<Scenario,TASKPlanning> scenarii;
    [SerializeField] public Scenario planningIndex = Scenario.Training;
    [SerializeField] public TASKPlanning planning;

    // Start is called before the first frame update
    void Start()
    {
        mb2 = MATBIISystem.Instance;
        scenarii = new Dictionary<Scenario, TASKPlanning>();
        scenarii.Add(Scenario.Training, Training);
        scenarii.Add(Scenario.A_B_AB, A_B_AB);
        scenarii.Add(Scenario.B_AB_A, B_AB_A);
        scenarii.Add(Scenario.AB_A_B, AB_A_B);
        scenarii.Add(Scenario.AB_B_A, AB_B_A);
        scenarii.Add(Scenario.B_A_AB, B_A_AB);
        scenarii.Add(Scenario.A_AB_B, A_AB_B);
        scenarii.Add(Scenario.DemoCNRS, DemoCNRS);
    }

    // Update is called once per frame
    void Update()
    {
        if (!mb2.started || !photonView.IsMine || mb2.training) return;

        if (SYSMON_index < planning.SYSMON_Tasks.Count)
        {
            if (planning.SYSMON_Tasks[SYSMON_index].startTime < mb2.elapsedTime)
            {
                bool[] tasks = {planning.SYSMON_Tasks[SYSMON_index].parameters.NormallyON,
                    planning.SYSMON_Tasks[SYSMON_index].parameters.NormallyOFF,
                    planning.SYSMON_Tasks[SYSMON_index].parameters.scale1,
                    planning.SYSMON_Tasks[SYSMON_index].parameters.scale2,
                    planning.SYSMON_Tasks[SYSMON_index].parameters.scale3,
                    planning.SYSMON_Tasks[SYSMON_index].parameters.scale4};

                bool[] directions = {Random.Range(float.MinValue, float.MaxValue) > 0.0f,
                    Random.Range(float.MinValue, float.MaxValue) > 0.0f,
                    Random.Range(float.MinValue, float.MaxValue) > 0.0f,
                    Random.Range(float.MinValue, float.MaxValue) > 0.0f};

                mb2.photonView.RPC ("SYSMON_Start", RpcTarget.All, tasks, directions);
                PhotonNetwork.SendAllOutgoingCommands();
                SYSMON_index++;
            }
        }
        if (COMM_index < planning.COMM_Tasks.Count)
        {
            if (planning.COMM_Tasks[COMM_index].startTime < mb2.elapsedTime)
            {
                mb2.photonView.RPC ("COMM_Start", RpcTarget.All, Random.Range(0, mb2.COMM_transmissions.Count));
                PhotonNetwork.SendAllOutgoingCommands();
                COMM_index++;
            }
        }
        if (TRACK_index < planning.TRACK_Tasks.Count)
        {
            if (planning.TRACK_Tasks[TRACK_index].startTime < mb2.elapsedTime)
            {
                mb2.photonView.RPC ("TRACK_Start", RpcTarget.All, Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
                PhotonNetwork.SendAllOutgoingCommands();
                TRACK_index++;
            }
        }
        if (RESMAN_index < planning.RESMAN_Tasks.Count)
        {
            if (planning.RESMAN_Tasks[RESMAN_index].startTime < mb2.elapsedTime)
            {
                mb2.photonView.RPC ("RESMAN_Start", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
                RESMAN_index++;
            }
        }
        if (WrkLd_index < planning.WrkLd_Tasks.Count)
        {
            if (planning.WrkLd_Tasks[WrkLd_index].startTime < mb2.elapsedTime)
            {
                mb2.photonView.RPC ("WrkLd_Start", RpcTarget.All);
                PhotonNetwork.SendAllOutgoingCommands();
                WrkLd_index++;
            }
        }

        //if (SYSMON_index >= planning.SYSMON_Tasks.Count && COMM_index >= planning.COMM_Tasks.Count && TRACK_index >= planning.TRACK_Tasks.Count && RESMAN_index >= planning.RESMAN_Tasks.Count)
        if (mb2.elapsedTime >= planning.duration
            && !mb2.isSYSMON_TASK_active()
            && !mb2.isCOMM_TASK_active()
            && !mb2.isTRACK_TASK_active()
            && !mb2.isRESMAN_TASK_active())
        {
            mb2.started = false;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)planningIndex);
            stream.SendNext(SYSMON_index);
            stream.SendNext(COMM_index);
            stream.SendNext(TRACK_index);
            stream.SendNext(RESMAN_index);
            stream.SendNext(WrkLd_index);
        }
        else
        {
            this.planningIndex = (Scenario)((int) stream.ReceiveNext());
            this.SYSMON_index = (int) stream.ReceiveNext();
            this.COMM_index = (int) stream.ReceiveNext();
            this.TRACK_index = (int) stream.ReceiveNext();
            this.RESMAN_index = (int) stream.ReceiveNext();
            this.WrkLd_index = (int) stream.ReceiveNext();
        }
    }

    public void resetIndexes()
    {
        SYSMON_index = 0;
        COMM_index = 0;
        TRACK_index = 0;
        RESMAN_index = 0;
        WrkLd_index = 0;
    }

    [ContextMenu("FORCE START")]
    public void StartTasks()
    {
        //if(mb2.started) return;
        resetIndexes();
        mb2.StartBattery();
    }
    
    /*
    [ContextMenu("DEBUG")]
    public void DEBUG()
    {
        print("DEBUG");
    }
    */
}
