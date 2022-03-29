using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ExperimenterControls : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Dropdown leftUser;
    [SerializeField] private TMPro.TMP_Dropdown rightUser;
    [SerializeField] private UnityEngine.UI.Toggle training;
    [SerializeField] private UnityEngine.UI.Toggle L_on, L_off, S_0, S_1, S_2, S_3;
    [SerializeField] private UnityEngine.UI.Toggle Nasa504;
    [SerializeField] private TMPro.TMP_Text radio, frequency;

    private ExperimentControls _instance;
    private ExperimentControls controls
    {
        get {
            if (_instance == null) _instance = GameObject.FindObjectOfType<ExperimentControls>();
            return _instance;
        }
    }
    private int playerNum = 0;

    private void Start()
    {
        PlayerManager pm = GetComponentInParent<PlayerManagerDesktop>();
        if (PlayerManager.LocalPlayerInstance != pm || !pm.NickName.Contains("XP")) this.gameObject.SetActive(false);
    }
    private void Update()
    {
        MATBIISystem.Instance.training = training.isOn;
        if (MATBIISystem.Instance != null && training.isOn)
        {
            MATBIISystem.AudioTransmission comm = MATBIISystem.Instance.getCOMM_onGoingTransmission();
            if (comm != null)
            {
                Nasa504.isOn = comm.isNASA504;
                radio.text = MATBIISystem.Instance.COMM_radioNames[comm.radio];
                frequency.text = comm.frequency.ToString("000.000", MATBIISystem.Instance.strFormat);
            }
        }

        if (playerNum != PhotonNetwork.PlayerList.Length)
        {
            playerNum = PhotonNetwork.PlayerList.Length;
            leftUser.options.Clear();
            rightUser.options.Clear();
            for (int p = 0; p < PhotonNetwork.PlayerList.Length; p++)
            {
                leftUser.options.Add(new TMPro.TMP_Dropdown.OptionData(PhotonNetwork.PlayerList[p].NickName));
                rightUser.options.Add(new TMPro.TMP_Dropdown.OptionData(PhotonNetwork.PlayerList[p].NickName));
            }
        }
    }

    public void GenerateAggregativeBoards() { controls.GenerateAggregativeBoards(); }
    public void ActivateAggregative(bool active) { controls.ActivateAggregative(active); }
    public void ActivateDistributive(bool active) { controls.ActivateDistributive(active); }
    public void ActivateSelfStatus(bool active) { controls.ActivateSelfStatus(active); }
    public void changeScenario(int index) { controls.scenario = (Scenario)index; }
    public void ChangeLeftPlayer(int index) { controls.SetLeftPlayer(PhotonNetwork.PlayerList[index].NickName); }
    public void ChangeRightPlayer(int index) { controls.SetRightPlayer(PhotonNetwork.PlayerList[index].NickName); }
    public void StartExperiment() { training.isOn = false; controls.StartExperiment(); }
    public void DivideTasks() { controls.DivideTasks(); }
    public void SYSMON_Start() { controls.SYSMON_Start(new bool[]{L_on.isOn, L_off.isOn, S_0.isOn, S_1.isOn, S_2.isOn, S_3.isOn}); }
    public void SYSMON_Stop() { controls.SYSMON_Stop(); }
    public void COMM_Start() { controls.COMM_Start(); }
    public void COMM_Stop() { controls.COMM_Stop(); }
    public void TRACK_Start() { controls.TRACK_Start(); }
    public void TRACK_Stop() { controls.TRACK_Stop(); }
    public void RESMAN_Start() { controls.RESMAN_Start(); }
    public void RESMAN_Stop() { controls.RESMAN_Stop(); }
    public void ALL_Start() { controls.ALL_Start(); }
    public void ALL_Stop() { controls.ALL_Stop(); }
    public void ALL_Reset() { controls.ALL_Reset(); }
}
