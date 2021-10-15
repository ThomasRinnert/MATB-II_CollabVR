using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionHistory : MonoBehaviour
{
    public List<ActivityWarning> warnings;

    private void Start()
    {
        warnings = new List<ActivityWarning>();
        warnings.AddRange(transform.GetComponentsInChildren<ActivityWarning>());
    }

    public void divideTasks(bool leftHalf)
    {
        foreach (var warning in warnings)
        {
            if (leftHalf)
            {
                if (warning is WarningTarget || warning is WarningTank) warning.gameObject.SetActive(false);
                else warning.gameObject.SetActive(true);
            }
            else
            {
                if (warning is WarningLights || warning is WarningScales || warning is WarningRadio) warning.gameObject.SetActive(false);
                else warning.gameObject.SetActive(true);
            }
        }
    }
}
