using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningTank : ActivityWarning
{
    [SerializeField] public MATBIISystem.RESMAN_Tank tank;

    [SerializeField] TextMeshPro value;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] GameObject gauge;
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
        progress.value = (float) MATBIISystem.Instance.getRESMAN_score() / 25.0f;
        passive_progress.value = (float)(MATBIISystem.Instance.elapsedTime / MATBIISystem.Instance.planner.planning.duration);

        //if (MATBII.isRESMAN_TASK_active())
        if (MATBII.started)
        {
            float fuel = MATBII.getRESMAN_tank(tank);
            float v = (MATBII.RESMAN_objective -500 < fuel && fuel < MATBII.RESMAN_objective + 500) ? 1.0f : (1 - Mathf.Abs((fuel - MATBII.RESMAN_objective) / MATBII.RESMAN_objective)); // * 1.25f;
            sprite.color = new Color(1.0f, v, v);

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
