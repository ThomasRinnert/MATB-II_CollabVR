using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.UI;

[RequireComponent(typeof(Replayer))]
public class Operator : MonoBehaviour
{
    [Header("Parameters")]
    
    [Range(0.5f, 2.0f)] [Tooltip("Body movement replay speed")]
    [SerializeField] public float speed = 1.0f;
    
    [Range(0.0f, 1.0f)] [Tooltip("Base success rate")]
    [SerializeField] public float accuracy = .5f;

    [Tooltip("Express the error rate as a function of stress")] 
    [SerializeField] public AnimationCurve errorFstress;
    
    [Range(0.0f, 1.0f)] [Tooltip("Amount of stress gained from each task")] 
    [SerializeField] public float stress_unit = 0.2f;
    [Range(0.0f, 1.0f)] [Tooltip("Slope of the stress cooldown function")] 
    [SerializeField] public float stress_cooldownRate = 1.0f;
    /*
    [Tooltip("Vary accuracy and speed depending on task")]
    [SerializeField] public double task_affinity;
    [Range(0.0f, 100.0f)] [Tooltip("Sample size (seconds)")]
    [SerializeField] public float timeWindow = 10.0f;
    */

    [Header("Attributes")]
    
    [Range(0.0f, 1.0f)] [Tooltip("Stress level")]
    [SerializeField] public float stress = 0.0f;
    
    /*
    [Range(0.0f, 1.0f)] [Tooltip("Proportion of the time window passed working")]
    [SerializeField] public float capacity = 0.0f;
    [SerializeField] private float capacityTimer = 0.0f;
    [SerializeField] private Queue<(bool,float)> capacityBuffer = new Queue<(bool,float)>();
    [SerializeField] private float capacityWorkTime = 0.0f;
    */

    [SerializeField] public Queue<MATBIISystem.MATBII_TASK> tasksQueue = new Queue<MATBIISystem.MATBII_TASK>();
    [SerializeField] public int SYSMON_todo = 0;
    [SerializeField] public int TRACK_todo = 0;
    [SerializeField] public int COMM_todo = 0;
    [SerializeField] public int RESMAN_todo = 0;
    [SerializeField] public List<(MATBIISystem.MATBII_TASK, bool, float)> tasksDone = new List<(MATBIISystem.MATBII_TASK, bool, float)>();
    [SerializeField] public int SYSMON_done = 0;
    [SerializeField] public int TRACK_done = 0;
    [SerializeField] public int COMM_done = 0;
    [SerializeField] public int RESMAN_done = 0;

    [Header("Links")]

    [SerializeField] public Replayer replayer;
    [SerializeField] public Animator animator;
    [SerializeField] public VRIK vRIK;
    
    [SerializeField] public Canvas canvas;
    //[SerializeField] public Slider capacitySlider;

    [ContextMenu("Bind")]
    void Bind()
    {
        if (replayer == null) replayer = GetComponent<Replayer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (vRIK == null) vRIK = GetComponentInChildren<VRIK>();

        if (canvas == null) canvas = GetComponentInChildren<Canvas>();
        //if (canvas != null && capacitySlider == null) capacitySlider = canvas.transform.Find("Panel/Capacity Progressbar/Front Slider").GetComponent<Slider>(); //canvas.GetComponentsInChildren<Slider>() ?
    }

    void Start()
    {
        Bind();
        vRIK.enabled = false;
    }

    public void GiveTask(MATBIISystem.MATBII_TASK task)
    {
        tasksQueue.Enqueue(task);
        switch (task)
        {
            case MATBIISystem.MATBII_TASK.SYSMON:
                SYSMON_todo++;
                break;
            case MATBIISystem.MATBII_TASK.TRACK:
                TRACK_todo++;
                break;
            case MATBIISystem.MATBII_TASK.COMM:
                COMM_todo++;
                break;
            case MATBIISystem.MATBII_TASK.RESMAN:
                RESMAN_todo++;
                break;
            default:
                Debug.LogError("Operator/GiveTask: Undefined task");
                return;
        }
        stress += 0.2f; // Should be progressive (coroutine ?)
    }

    IEnumerator Increase()
    {
        
        yield return new WaitForSeconds(1.0f);
    }

    void Update()
    {
        // Capacity
        //ProcessCapacity();

        // Stress
        // stress = min ( max ( stress +  ( chargerate * speed - cooldown ) * Time.deltaTime), 0), 1.0f);
        if (stress > 0)
        {
            stress -= Time.deltaTime /* speed*/ * 0.01f; // MAGIC NUMBER ! YAY !
            if (stress < 0) stress = 0;
        }

        // Task chainning
        if (tasksQueue.Count > 0 && replayer.state != Replayer.ReplayState.Playing && replayer.state != Replayer.ReplayState.Paused)
        {
            animator.SetBool("IK", true);
            vRIK.enabled = true;
            var task = tasksQueue.Dequeue();
            replayer.Play(task, default, default, onReplayEnded);
        }

        // Idle
        if (animator.GetBool("IK") == true)
        {
            if (tasksQueue.Count <= 0
            && replayer.state != Replayer.ReplayState.Playing
            && replayer.state != Replayer.ReplayState.Paused)
            {
                animator.SetBool("IK", false);
                vRIK.enabled = false;
            }
        }
    }

    /*
    void ProcessCapacity()
    {
        totalTime += Time.deltaTime;
        if (replayer.state == Replayer.ReplayState.Playing) { workingTime += Time.deltaTime; }
        capacity = workingTime / totalTime;
        */
        

        /*
        capacityTimer += Time.deltaTime;
        capacityBuffer.Enqueue(((replayer.state == Replayer.ReplayState.Playing), Time.deltaTime));

        while (capacityTimer >= timeWindow) { capacityTimer -= capacityBuffer.Dequeue().Item2; }
        
        (bool,float)[] capacityArray = capacityBuffer.ToArray(); capacityWorkTime = 0.0f;
        for (int i = 0; i < capacityBuffer.Count; i++)
        {
            if (capacityArray[i].Item1) capacityWorkTime += capacityArray[i].Item2;
        }
        
        capacity = capacityWorkTime / capacityTimer;
        capacitySlider.value = capacity;
    }
    */

    void onReplayEnded(MATBIISystem.MATBII_TASK task)
    {
        // Score / Error
        float errorChance = errorFstress.Evaluate(stress);
        float score = 1.0f - errorChance;
        bool success = Random.Range(0.0f, 1.0f) > errorChance;
        Debug.Log("Succes chance after stress evaluation: " + score.ToString());

        switch (task)
        {
            case MATBIISystem.MATBII_TASK.SYSMON:
                SYSMON_todo--; SYSMON_done++;
                break;
            case MATBIISystem.MATBII_TASK.TRACK:
                TRACK_todo--; TRACK_done++;
                break;
            case MATBIISystem.MATBII_TASK.COMM:
                COMM_todo--; COMM_done++;
                break;
            case MATBIISystem.MATBII_TASK.RESMAN:
                RESMAN_todo--; RESMAN_done++;
                break;
            default:
                Debug.LogError("Operator/GiveTask: Undefined task");
                return;
        }

        tasksDone.Add((task, success, score));
    }
}
