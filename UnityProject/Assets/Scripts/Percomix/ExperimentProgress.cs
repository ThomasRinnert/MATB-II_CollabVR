using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExperimentProgress : MonoBehaviour
{
    public ExperimentControlsMATBIITeam control;
    public TextMeshPro text;

    System.Globalization.CultureInfo strFormat = System.Globalization.CultureInfo.InvariantCulture;

    // Update is called once per frame
    void Update()
    {
        int remainingTasks = control.getSpawner().transform.childCount;
        for (int i = control.getSpawner().index; i < control.getSpawner().schedule.IncomingTasks.Count; i++)
        {
            if(!control.getSpawner().started) break;
            remainingTasks += control.getSpawner().schedule.IncomingTasks[i].numberOfTask;
        }

        text.text = "Team score: "
        + control.getTeamScore().ToString("###0.0", strFormat) + " / "
        + control.getMaxScore().ToString("###0", strFormat) + "\n"
        +"Remaining tasks: "
        + remainingTasks.ToString("###0", strFormat) + "\n"
        +"Elapsed time: "
        + control.getElapsedTime().TotalMinutes.ToString("###0", strFormat) + ":"
        + control.getElapsedTime().Seconds.ToString("###0", strFormat);
    }
}