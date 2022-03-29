using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionHistory : MonoBehaviour
{
    public List<ActivityWarning> warnings;
    public Slider progress;
    public Slider passiveProgress;

    private void Start()
    {
        warnings = new List<ActivityWarning>();
        warnings.AddRange(transform.GetComponentsInChildren<ActivityWarning>());
    }

    private void Update()
    {
        progress.value = (float) (MATBIISystem.Instance.MATBII_score / 100.0);
        passiveProgress.value = ((float) MATBIISystem.Instance.elapsedTime / (float) MATBIISystem.Instance.planner.planning.duration);
    }

    public void divideTasks(bool leftHalf)
    {
        foreach (var warning in warnings)
        {
            if (leftHalf)
            {
                if (warning is WarningRadio || warning is WarningTank) warning.gameObject.SetActive(false);
                else warning.gameObject.SetActive(true);
            }
            else
            {
                if (warning is WarningLights || warning is WarningScales || warning is WarningTarget) warning.gameObject.SetActive(false);
                else warning.gameObject.SetActive(true);
            }
        }
    }

    public void enableTasks(ActivityWarning task)
    {
        foreach (var warning in warnings)
        {
            if (task is WarningLights && warning is WarningLights) warning.gameObject.SetActive(true);
            else if (task is WarningScales && warning is WarningScales) warning.gameObject.SetActive(true);
            else if (task is WarningTarget && warning is WarningTarget) warning.gameObject.SetActive(true);
            else if (task is WarningRadio && warning is WarningRadio) warning.gameObject.SetActive(true);
            else if (task is WarningTank && warning is WarningTank) warning.gameObject.SetActive(true);
            else warning.gameObject.SetActive(false);
        }
    }
    public void disableTasks(ActivityWarning task)
    {
        foreach (var warning in warnings)
        {
            if (task is WarningLights && warning is WarningLights) warning.gameObject.SetActive(false);
            else if (task is WarningScales && warning is WarningScales) warning.gameObject.SetActive(false);
            else if (task is WarningTarget && warning is WarningTarget) warning.gameObject.SetActive(false);
            else if (task is WarningRadio && warning is WarningRadio) warning.gameObject.SetActive(false);
            else if (task is WarningTank && warning is WarningTank) warning.gameObject.SetActive(false);
            else warning.gameObject.SetActive(true);
        }
    }

    public void clearTaskDivision() { foreach (var warning in warnings) { warning.gameObject.SetActive(true); } }
}
