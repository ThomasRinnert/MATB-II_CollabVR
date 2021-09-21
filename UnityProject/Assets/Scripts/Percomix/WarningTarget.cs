using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningTarget : ActivityWarning
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
        if (MATBII.isTRACK_TASK_active())
        {
            float timer = MATBII.getTRACK_timer();
            float sucess = MATBII.getTRACK_successTimer();
            float v = (100.0f * sucess) / timer;
            sprite.color = new Color(1.0f, v/100.0f, v/100.0f);
            value.text = v.ToString("0.0") + "%";
        }
        else
        {
            sprite.color = Color.white;
            value.text = "";
        }
    }
}
