using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class WarningLights : ActivityWarning
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
