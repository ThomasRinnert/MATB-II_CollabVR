using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressHalo : MonoBehaviour
{
    [SerializeField] Material StressVeryLow;
    [SerializeField] Material StressLow;
    [SerializeField] Material StressMedium;
    [SerializeField] Material StressHigh;
    [SerializeField] Material StressVeryHigh;

    [SerializeField] Renderer currentStress;
    [SerializeField] Renderer predictedStress;
    [SerializeField] Renderer newPredictedStress;

    [SerializeField] GameObject predictedStressOutline;

    private Operator op;

    void Start()
    {
        op = GetComponentInParent<Operator>();
        if (op == null) Debug.LogError("Operator not found");
    }

    void Update()
    {
        // Curent Stress
        currentStress.material = MaterialFromStressLevel(op.stressLevel);

        // Predicted stress at the end of the task batch
        predictedStress.material = MaterialFromStressLevel(op.predictedStressLevel);

        // Predicted stress + added stress by tasks grabbed by the user 
        if (ExperimentControlsMATBIITeam.Instance.taskAssigning)
        {
            float stressAmount =
                op.predictedStress
                + (ExperimentControlsMATBIITeam.Instance.LeftHand()? op.StressFromTask(ExperimentControlsMATBIITeam.Instance.LeftTask()) : 0.0f)
                + (ExperimentControlsMATBIITeam.Instance.RightHand()? op.StressFromTask(ExperimentControlsMATBIITeam.Instance.RightTask()) : 0.0f);
            newPredictedStress.enabled = true;
            newPredictedStress.material = MaterialFromStressLevel(op.StressLevelFromFloat(stressAmount));
        }
        else
        {
            newPredictedStress.enabled = false;
            //r.material.SetColor("_EmissionColor", Color.gray);
        }
    }

    public void ShowPrediction(bool show)
    {
        predictedStress.gameObject.SetActive(show);
        newPredictedStress.gameObject.SetActive(show);

        predictedStressOutline.SetActive(show);
    }

    private Material MaterialFromStressLevel(Operator.StressLevel lvl)
    {
        if (lvl == Operator.StressLevel.VLow) return StressVeryLow;
        else if (lvl == Operator.StressLevel.Low) return StressLow;
        else if (lvl == Operator.StressLevel.Medium) return StressMedium;
        else if (lvl == Operator.StressLevel.High) return StressHigh;
        else if (lvl == Operator.StressLevel.VHigh) return StressVeryHigh;
        else return null;
    }
}
