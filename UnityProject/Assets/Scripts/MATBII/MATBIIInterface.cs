using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MATBIIInterface : MonoBehaviour
{
    [SerializeField] public MATBIISystem MATBII;

    [Header("SYSMON references")]
    [SerializeField] public SYSMONSwitch SYSMON_normallyON_Switch;
    [SerializeField] public SYSMONSwitch SYSMON_normallyOFF_Switch;
    [SerializeField] public List<SYSMONScale> SYSMON_Scales;

    [Header("COMM references")]
    [SerializeField] public List<COMMRadio> COMM_radios;
    [SerializeField] public AudioSource COMM_audio;

    [Header("TRACK references")]
    [SerializeField] public TextMesh TRACK_AutomodeDisplay;
    [SerializeField] public Collider TRACK_targetContainer;
    [SerializeField] public TRACKTarget TRACK_target;

    [Header("RESMAN references")]
    [SerializeField] public List<TextMesh> RESMAN_Tanks;
    [SerializeField] public List<RESMANPumpButton> RESMAN_PumpButtons;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
