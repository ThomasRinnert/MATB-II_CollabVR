using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningScales : ActivityWarning
{
    [SerializeField] TextMeshPro value_0;
    [SerializeField] TextMeshPro value_1;
    [SerializeField] TextMeshPro value_2;
    [SerializeField] TextMeshPro value_3;
    [SerializeField] SpriteRenderer sprite_0;
    [SerializeField] SpriteRenderer sprite_1;
    [SerializeField] SpriteRenderer sprite_2;
    [SerializeField] SpriteRenderer sprite_3;
    MATBIISystem MATBII;

    // Start is called before the first frame update
    void Start()
    {
        MATBII = MATBIISystem.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (MATBII.isSYSMON_Scales_active())
        {
            float[] timers = {MATBII.getSYSMON_Scales_timers()[0] + MATBII.getSYSMON_Scales_criticalTimers()[0],
                MATBII.getSYSMON_Scales_timers()[1] + MATBII.getSYSMON_Scales_criticalTimers()[1],
                MATBII.getSYSMON_Scales_timers()[2] + MATBII.getSYSMON_Scales_criticalTimers()[2],
                MATBII.getSYSMON_Scales_timers()[3] + MATBII.getSYSMON_Scales_criticalTimers()[3]};
            float[] v = {(100.0f * timers[0]) / MATBII.SYSMON_timeLimit,
                (100.0f * timers[1]) / MATBII.SYSMON_timeLimit,
                (100.0f * timers[2]) / MATBII.SYSMON_timeLimit,
                (100.0f * timers[3]) / MATBII.SYSMON_timeLimit};
            sprite_0.color = new Color(1.0f, 1.0f - v[0]/100.0f, 1.0f - v[0]/100.0f);
            sprite_1.color = new Color(1.0f, 1.0f - v[1]/100.0f, 1.0f - v[1]/100.0f);
            sprite_2.color = new Color(1.0f, 1.0f - v[2]/100.0f, 1.0f - v[2]/100.0f);
            sprite_3.color = new Color(1.0f, 1.0f - v[3]/100.0f, 1.0f - v[3]/100.0f);
            value_0.text = (MATBII.SYSMON_timeLimit - timers[0]).ToString("0.0");
            value_1.text = (MATBII.SYSMON_timeLimit - timers[1]).ToString("0.0");
            value_2.text = (MATBII.SYSMON_timeLimit - timers[2]).ToString("0.0");
            value_3.text = (MATBII.SYSMON_timeLimit - timers[3]).ToString("0.0");
        }
        else
        {
            sprite_0.color = Color.white; sprite_1.color = Color.white; sprite_2.color = Color.white; sprite_3.color = Color.white;
            value_0.text = ""; value_1.text = ""; value_2.text = ""; value_3.text = "";
        }
    }
}
