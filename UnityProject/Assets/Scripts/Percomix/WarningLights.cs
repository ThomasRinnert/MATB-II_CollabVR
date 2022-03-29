using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class WarningLights : ActivityWarning
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
        progress.value = (float) MATBIISystem.Instance.getSYSMON_score() / 25.0f;
        passive_progress.value = ((float) MATBIISystem.Instance.planner.SYSMON_index) / ((float) MATBIISystem.Instance.planner.planning.SYSMON_Tasks.Count);

        if (MATBII.isSYSMON_NormallyON_active() || MATBII.isSYSMON_NormallyOFF_active())
        {
            float timer = Mathf.Max(MATBII.getSYSMON_NormallyON_timer(), MATBII.getSYSMON_NormallyOFF_timer());
            float v = (100.0f * timer) / MATBII.SYSMON_timeLimit;
            sprite.color = new Color(1.0f, 1.0f - v/100.0f, 1.0f - v/100.0f);
            value.text = (MATBII.SYSMON_timeLimit - timer).ToString("0.0");
        }
        else
        {
            sprite.color = Color.white;
            value.text = "";
        }
    }
}
