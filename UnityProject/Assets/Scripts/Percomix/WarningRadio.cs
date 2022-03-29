using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningRadio : ActivityWarning
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
        progress.value = (float) MATBIISystem.Instance.getCOMM_score() / 25.0f;
        passive_progress.value = ((float) MATBIISystem.Instance.planner.COMM_index / (float) MATBIISystem.Instance.planner.planning.COMM_Tasks.Count);

        if (MATBII.isCOMM_TASK_active())
        {
            float timer = MATBII.getCOMM_timer();
            float v = (100.0f * timer) / MATBII.COMM_timeLimit;
            sprite.color = new Color(1.0f, 1.0f - v/100.0f, 1.0f - v/100.0f);
            value.text = (MATBII.COMM_timeLimit - timer).ToString("0.0");
        }
        else
        {
            sprite.color = Color.white;
            value.text = "";
        }
    }
}
