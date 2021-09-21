using UnityEngine;
using UnityEngine.UI;

public class RESMANTank : MonoBehaviour
{
    private MATBIISystem MATBII;
    [SerializeField] public MATBIISystem.RESMAN_Tank ID;
    [SerializeField] public TextMesh text;
    [SerializeField] public Slider slider;

    void Start()
    {
        if (text == null) text = GetComponent<TextMesh>();
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
    }   
}