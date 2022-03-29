using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XP : MonoBehaviour
{
    [SerializeField]
    public TASKPlanner planner;
    [SerializeField]
    public GameObject TRACK;
    [SerializeField]
    public GameObject SYSMON;

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerManager.LocalPlayerInstance.NickName.Contains("Unity"))
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(planner.TRACK_index % 2 > 0)
        {
            TRACK.gameObject.SetActive(false);
        }
        else if(planner.TRACK_index % 2 == 0)
        {
            TRACK.gameObject.SetActive(true);
        }
        if(planner.SYSMON_index % 3 > 0)
        {
            SYSMON.gameObject.SetActive(false);                                      
        }
        else if(planner.SYSMON_index % 3 == 0)
        {
            SYSMON.gameObject.SetActive(true);
        }
    }
}
