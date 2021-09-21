using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningScales : ActivityWarning
{
    [SerializeField] TextMeshPro value;
    [SerializeField] SpriteRenderer sprite;
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
            float timer = Mathf.Max(MATBII.getSYSMON_Scales_timers()) + Mathf.Max(MATBII.getSYSMON_Scales_criticalTimers());
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
