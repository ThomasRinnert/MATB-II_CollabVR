using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WarningTank : ActivityWarning
{
    [SerializeField] public MATBIISystem.RESMAN_Tank tank;

    [SerializeField] TextMeshPro value;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] GameObject gauge;
    MATBIISystem MATBII;

    // Start is called before the first frame update
    void Start()
    {
        MATBII = MATBIISystem.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (MATBII.isRESMAN_TASK_active())
        {
            float fuel = MATBII.getRESMAN_tank(tank);
            float v = (100 * fuel) / MATBII.RESMAN_objective;
            sprite.color = new Color(1.0f, v/100.0f, v/100.0f);
            float c = (180 * fuel) / MATBII.getRESMAN_capacity(tank) - 90.0f;
            gauge.transform.localRotation = Quaternion.Euler(0,0,-c);
            value.text = fuel.ToString("0");
        }
        else
        {
            sprite.color = Color.white;
            gauge.transform.localRotation = Quaternion.Euler(0,0,-90);
            value.text = "";
        }
    }
}
