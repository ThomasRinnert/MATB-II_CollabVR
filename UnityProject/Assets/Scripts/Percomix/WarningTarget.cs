using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningTarget : ActivityWarning
{
    [SerializeField] TextMeshPro value;
    [SerializeField] SpriteRenderer sprite;
    MATBIISystem MATBII;
    public Slider progress;
    public Slider passive_progress;

    // Start is called before the first frame update
    void Start()
    {
        MATBII = MATBIISystem.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        progress.value = (float) MATBIISystem.Instance.getTRACK_score() / 25.0f;
        passive_progress.value = ((float) MATBIISystem.Instance.planner.TRACK_index / (float) MATBIISystem.Instance.planner.planning.TRACK_Tasks.Count);

        if (MATBII.isTRACK_TASK_active())
        {
            // float timer = MATBII.getTRACK_timer();
            // float sucess = MATBII.getTRACK_successTimer();
            // float v = (100.0f * sucess) / timer;

            float max = MATBII.interfaces[0].TRACK_target.MaxDistance();
            float val = MATBII.interfaces[0].TRACK_target.MeanDistance();
            float v = 1.0f - (val / max);

            sprite.color = new Color(1.0f, v, v);
            value.text = (v * 100.0f).ToString("0.0") + "%";
        }
        else
        {
            sprite.color = Color.white;
            value.text = "";
        }
    }
}
