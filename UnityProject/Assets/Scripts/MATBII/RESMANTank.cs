using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RESMANTank : MonoBehaviour
{
    private MATBIISystem MATBII;
    [SerializeField] public MATBIISystem.RESMAN_Tank ID;
    [SerializeField] public TextMeshPro text;
    [SerializeField] public Slider slider;
    [SerializeField] public Image gauge;

    void Start()
    {
        if (text == null) text = GetComponent<TextMeshPro>();
        if (slider == null) slider = GetComponentInChildren<Slider>();    
        MATBII = MATBIISystem.Instance;
    }

    void Update()
    {
        // No need to update the unlimited tanks
        if (ID == MATBIISystem.RESMAN_Tank.D || ID == MATBIISystem.RESMAN_Tank.F) return;

        float fuel = MATBII.getRESMAN_tank(ID);

        text.text = fuel.ToString("0", MATBII.strFormat);
        slider.value = fuel / MATBII.getRESMAN_capacity(ID);
        
        if (ID == MATBIISystem.RESMAN_Tank.A || ID == MATBIISystem.RESMAN_Tank.B)
        {
            if (MATBII.RESMAN_objective - 500.0f < fuel && fuel < MATBII.RESMAN_objective + 500.0f)
            {
                if (gauge.color != Color.green) gauge.color = Color.green;
            }
            else
            {
                if (gauge.color != Color.red) gauge.color = Color.red;
            }
        }
    }
}