using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class AggregativeUserBoard : MonoBehaviour
{
    [SerializeField] public GameObject AggregativeLinePrefab;
    [SerializeField] public List<GameObject> panels;

    public static AggregativeUserBoard Instance;
    void Start()
    {
        Instance = this;
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
    }

    [ContextMenu("GENERATE PANELS")]
    public void Generate()
    {
        foreach (var panel in panels)
        {
            for (int i = 0; i < panel.transform.childCount; i++)
            {
                Destroy(panel.transform.GetChild(i).gameObject);
            }
            var index = 0;
            for (int p = 0; p < GameManager.Instance.players.Count /*GameManager.Instance.players.Count*/; p++)
            {
                string name = GameManager.Instance.players[p].NickName;
                if (name.Contains("XP") || name.Contains("CAM")) continue;
                
                GameObject line = Instantiate(AggregativeLinePrefab, panel.transform, false);
                UserBoard b = line.GetComponent<UserBoard>();
                Photon.Realtime.Player player = GameManager.Instance.players[p].player;
                b.player = player;
                b.player_manager =  GameManager.Instance.players[p];
                line.GetComponentInChildren<UserID>().Init();
                line.transform.localPosition = new Vector3(0, -index, 0);
                index++;
            }
        }
    }

    [ContextMenu("ACTIVATE")]
    public void Activate(bool active)
    {
        PlayerManager pm = GetComponentInParent<PlayerManager>();
        foreach (var panel in panels)
        {
            if (pm.NickName.Contains("Cam")) panel.SetActive(false);
            else panel.SetActive(active);
        }
    }

    public void DivideTasks(string leftPlayer, string rightPlayer)
    {
        foreach (var panel in panels)
        {
            for (int i = 0; i < panel.transform.childCount; i++)
            {
                var board = panel.transform.GetChild(i);
                if(board.GetComponent<UserBoard>().player_manager.NickName == leftPlayer)
                {
                    board.GetComponentInChildren<ActionHistory>().divideTasks(true);
                }
                if(board.GetComponent<UserBoard>().player_manager.NickName == rightPlayer)
                {
                    board.GetComponentInChildren<ActionHistory>().divideTasks(false);
                }
            }
        }
    }
    public void clearTaskDivision()
    {
        foreach (var panel in panels)
        {
            for (int i = 0; i < panel.transform.childCount; i++)
            {
                var board = panel.transform.GetChild(i);
                board.GetComponentInChildren<ActionHistory>().clearTaskDivision();
            }
        }
    }
}
