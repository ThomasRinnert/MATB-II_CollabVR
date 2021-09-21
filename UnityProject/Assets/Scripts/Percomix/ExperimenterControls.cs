using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimenterControls : MonoBehaviour
{
    ExperimentControls _instance;
    ExperimentControls controls
    {
        get {
            if (_instance == null) _instance = GameObject.FindObjectOfType<ExperimentControls>();
            return _instance;
        }
    }

    public void changeScenario(int index)
    {
        controls.scenario = (Scenario)index;
    }

    public void GenerateAggregativeBoards()
    {
        controls.GenerateAggregativeBoards();
    }
    public void ActivateAggregative(bool active)
    {
        controls.ActivateAggregative(active);
    }
    public void ActivateDistributive(bool active)
    {
        controls.ActivateDistributive(active);
    }
    public void ActivateSelfStatus(bool active)
    {
        controls.ActivateSelfStatus(active);
    }
    public void StartExperiment()
    {
        controls.StartExperiment();
    }
}
