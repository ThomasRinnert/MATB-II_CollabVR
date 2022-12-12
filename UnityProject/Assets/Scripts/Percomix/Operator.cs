using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Replayer))]
public class Operator : MonoBehaviour
{
    [Header("Parameters")]

    [Tooltip("Leave -1 for automatic attribution")]
    public string ID = "-1";
    public static int _ID = 0;
    
    [Range(0.5f, 2.0f)] [Tooltip("Body movement replay speed")]
    public float speed = 1.0f;
    
    [Range(0.0f, 1.0f)] [Tooltip("Base success rate")]
    public float accuracy = .5f;

    [Tooltip("Express the error rate as a function of stress")] 
    public AnimationCurve errorFstress;
    public double errorProbabilityFunction(float x)
    {
        // Error function shifted and scaled
        // 2/sqrt(pi)*integral(exp(-t**2), t=0..z)
        // C# don't handle integral so I use a numeric aproximation of the error function (cf ErrorFunction.cs)
        return ErrorFunction.erf((x - 0.65) / 0.2) * 0.5 + 0.5;
    }
    
    [Range(0.0f, 1.0f)] [Tooltip("Amount of stress gained from each task")] 
    public float stress_unit = 0.2f;
    [Range(0.0f, 1.0f)] [Tooltip("Slope of the stress cooldown function")] 
    public float stress_cooldownRate = 0.001f;
    [Range(0.0f, 1.0f)] [Tooltip("Threshold between low and medium levels of stress")] 
    public float stress_LMThreshold = 0.4f;
    [Range(0.0f, 1.0f)] [Tooltip("Threshold between medium and high levels of stress")] 
    public float stress_MHThreshold = 0.7f;
    
    /*
    [Range(0.0f, 100.0f)] [Tooltip("Sample size (seconds)")]
    [SerializeField] public float timeWindow = 10.0f;
    */

    [Header("Attributes")]
    
    [Range(0.0f, 1.0f)] [Tooltip("Stress level")]
    public float stress = 0.0f;
    public enum StressLevel { Low, Medium, High }
    public class StressRecord
    {
        public StressLevel level;
        public float average;
        public float duration;
    }
    public List<(float, float)> stressOverTime = new List<(float, float)>(); // (stress, deltaTime)
    public StressLevel stressLevel = StressLevel.Low;
    public List<StressRecord> stressRecords = new List<StressRecord>();
    public void ResetStress() {stress = 0; stressLevel = StressLevel.Low; stressOverTime.Clear(); stressRecords.Clear(); GetComponentInChildren<StressOverTimeGraph>(true).ResetGraph(); }
    
    [Range(0.0f, 1.0f)] [Tooltip("Proportion of the time window passed working")]
    [SerializeField] public float timeWorkedRatio = 0.0f;
    double totalTime = 0;
    double workingTime = 0;
    public void ResetWorkingTime() { totalTime = 0; workingTime = 0; timeWorkedRatio = 0.0f; }

    public Queue<MATBIISystem.MATBII_TASK> tasksQueue = new Queue<MATBIISystem.MATBII_TASK>();
    public int SYSMON_todo = 0;
    public int TRACK_todo = 0;
    public int COMM_todo = 0;
    public int RESMAN_todo = 0;
    public int todoBatch = 0;
    public int taskTodoCount() { return SYSMON_todo + TRACK_todo + COMM_todo + RESMAN_todo; }
    public void ResetTaskQueue() { SYSMON_todo = 0; TRACK_todo = 0; COMM_todo = 0; RESMAN_todo = 0; todoBatch = 0; tasksQueue.Clear(); }

    public List<(MATBIISystem.MATBII_TASK, bool, float)> tasksDone = new List<(MATBIISystem.MATBII_TASK, bool, float)>();
    public int SYSMON_done = 0;
    public int TRACK_done = 0;
    public int COMM_done = 0;
    public int RESMAN_done = 0;
    public int taskDoneCount() { return SYSMON_done + TRACK_done + COMM_done + RESMAN_done; }
    public void ResetTaskHistory() { SYSMON_done = 0; TRACK_done = 0; COMM_done = 0; RESMAN_done = 0; tasksDone.Clear(); }

    [Header("Links")]

    public ExperimentControlsMATBIITeam control = null;
    public Replayer replayer;
    public Animator animator;
    public VRIK vRIK;
    
    public Canvas canvas;
    public Outline outline;

    [ContextMenu("Bind")]
    void Bind()
    {
        if (replayer == null) replayer = GetComponent<Replayer>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (vRIK == null) vRIK = GetComponentInChildren<VRIK>();

        if (canvas == null) canvas = GetComponentInChildren<Canvas>();
        if (outline == null) outline = GetComponentInChildren<Outline>();
    }

    void Start()
    {
        Bind();
        vRIK.enabled = false;

        if(ID.Contains("-1"))
        {
            ID = _ID.ToString();
            _ID++;
        }
    }

    public void GiveRandomTask(int numberOfTask = 1)
    {
        for (int i = 0; i < numberOfTask; i++)
        {
            GiveTask((MATBIISystem.MATBII_TASK)UnityEngine.Random.Range(0,4));
        } 
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
        todoBatch++;
    }

    void Update()
    {
        if (control.isRunning()) ProcessTimeWorking();

        if (control.isRunning()) ProcessStress();

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

    void onReplayEnded(MATBIISystem.MATBII_TASK task)
    {
        // Score / Error
        float errorChance = (float) errorProbabilityFunction(stress); //errorFstress.Evaluate(stress);
        float score = 1.0f - errorChance;
        bool success = UnityEngine.Random.Range(0.0f, 1.0f) > errorChance;
        //Debug.Log("Stress: " + stress.ToString() + " | Score: " + score.ToString() + " | " + ((success) ? "Success" : "Failure") );

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
        if (tasksQueue.Count <= 0) todoBatch = 0;
    }

    public float getProgress()
    {
        return 1.0f - ((taskTodoCount() - replayer.getProgress()) / (todoBatch));
    }

    public float getScore()
    {
        float score = 0.0f;
        foreach (var task in tasksDone)
        {
            score += task.Item3;
        }
        return score;
    }

    public float getScoreAvg()
    {
        return (tasksDone.Count > 0) ? getScore() / (float)(tasksDone.Count) : 0.0f;
    }

    public int getSuccess()
    {
        int success = 0;
        foreach (var task in tasksDone)
        {
            success += task.Item2 ? 1 : 0;
        }
        return success;
    }

    public int getFailure()
    {
        int fail = 0;
        foreach (var task in tasksDone)
        {
            fail += task.Item2 ? 0 : 1;
        }
        return fail;
    }

    public float getSuccessRate()
    {
        return (tasksDone.Count > 0) ? getSuccess() / (float)(tasksDone.Count) : 0.0f;
    }
    
    /*
    [SerializeField] private float capacityTimer = 0.0f;
    [SerializeField] private Queue<(bool,float)> capacityBuffer = new Queue<(bool,float)>();
    [SerializeField] private float capacityWorkTime = 0.0f;
    */

    public bool isWorking() { return replayer.state == Replayer.ReplayState.Playing;}

    void ProcessTimeWorking()
    {
        totalTime += Time.deltaTime;
        if (isWorking()) { workingTime += Time.deltaTime; }
        timeWorkedRatio = (float)(workingTime / totalTime);

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
        */
    }

    void ProcessStress()
    {
        float stress_chargerate = replayer.state == Replayer.ReplayState.Playing ? stress_unit / 10.0f : 0.0f; // one unit over 10 seconds of task
        stress = Mathf.Min( Mathf.Max( (stress + ( stress_chargerate * speed - stress_cooldownRate ) * Time.deltaTime), 0), 1.0f);

        stressOverTime.Add((stress, Time.deltaTime));

        if (stressLevel == StressLevel.Low && stress > stress_LMThreshold)
        {
            RecordStress();
            stressLevel = StressLevel.Medium;
        }
        else if (stressLevel == StressLevel.Medium && stress > stress_MHThreshold)
        {
            RecordStress();
            stressLevel = StressLevel.High;
        }
        else if (stressLevel == StressLevel.High && stress < stress_MHThreshold)
        {
            RecordStress();
            stressLevel = StressLevel.Medium;
        }
        else if (stressLevel == StressLevel.Medium && stress < stress_LMThreshold)
        {
            RecordStress();
            stressLevel = StressLevel.Low;
        }
    }

    void RecordStress()
    {
        float stressSum = 0.0f;
        float timeSum = 0.0f;
        for (int i = 0; i < stressOverTime.Count; i++)
        {
            stressSum += stressOverTime[i].Item1 * stressOverTime[i].Item2;
            timeSum += stressOverTime[i].Item2;
        }

        StressRecord record = new StressRecord();
        record.level = stressLevel;
        record.average = stressSum / timeSum;
        record.duration = timeSum;
        stressRecords.Add(record);

        stressOverTime.Clear();
    }
}

public static class StressEnumExtension
{
    public static string String(this Operator.StressLevel lvl)
    {
        string level = "ERROR";
        if (lvl == Operator.StressLevel.Low) level = "LOW";
        else if (lvl == Operator.StressLevel.Medium) level = "MEDIUM";
        else if (lvl == Operator.StressLevel.High) level = "HIGH";
        return level;
    }
}
