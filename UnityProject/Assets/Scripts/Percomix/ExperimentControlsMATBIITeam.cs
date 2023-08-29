using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ConditionEnumExtension
{
    public static string String(this ExperimentControlsMATBIITeam.Condition c)
    {
        /*
        string cond = "";
        if (Symbolic) {cond += "Symbolic";} if (Literal) {cond += "Literal";}
        if (Stress) {cond += "Stress";} if (!Stress && (Literal || Symbolic)) {cond += "NoStress";}
        if (!Symbolic && !Literal && !Stress) {cond += "NoCue";}
        return cond;
        */
        switch (c)
        {
            case ExperimentControlsMATBIITeam.Condition.C0: return "C0";
            case ExperimentControlsMATBIITeam.Condition.C1: return "3cNP"; // 3 colors - no prediction
            case ExperimentControlsMATBIITeam.Condition.C2: return "5cNP"; // 5 colors - no prediction
            case ExperimentControlsMATBIITeam.Condition.C3: return "3cWP"; // 3 colors - with prediction
            case ExperimentControlsMATBIITeam.Condition.C4: return "5cWP"; // 5 colors - with prediction
            default: return "UnknownCondition";
        }
    }
}

public class ExperimentControlsMATBIITeam : MonoBehaviour
{
    [SerializeField] int sessionID = -1;
    [SerializeField] int trialID = -1;
    
    public enum Condition { C0, C1, C2, C3, C4 }
    public Condition condition = Condition.C0;

    public enum ScenarioType { OneBatch, InitialTraining, TrialTraining, Trial, Infinite }
    public ScenarioType scenario = ScenarioType.OneBatch;

    /* TODO
    LOG Head movements
    */

    #region ObjectsBinding
    public static ExperimentControlsMATBIITeam Instance = null;
    [SerializeField] Transform opsRoot = null;
    [SerializeField] TaskSpawning spawner = null; public TaskSpawning getSpawner() { return spawner; }
    [SerializeField] List<Operator> operators = null;
    [SerializeField] OperatorStatusPanel[] panels = null;

    [ContextMenu("BIND")]
    public void Bind()
    {
        Instance = this;

        if (opsRoot == null)
        {
            GameObject go = GameObject.Find("Operators");
            if (go != null) opsRoot = go.transform;
            else { Debug.LogError("Operators root transform not found"); return; }
        }

        operators = new List<Operator>(opsRoot.GetComponentsInChildren<Operator>());
        operators.Sort((op1,op2)=>op1.ID.CompareTo(op2.ID));

        panels = opsRoot.GetComponentsInChildren<OperatorStatusPanel>();
    }
    #endregion

    #region ConditionSelection
    public void changeCondition(Condition c)
    {
        condition = c;
        switch(c)
        {
            case Condition.C0:
            case Condition.C1:
                foreach (var op in operators)
                {
                    StressHalo halo = op.GetComponentInChildren<StressHalo>();
                    op.stress_VLLThreshold = 0.33f;
                    op.stress_LMThreshold = 0.33f;
                    op.stress_MHThreshold = 0.66f;
                    op.stress_HVHThreshold = 0.66f;
                    if (halo != null) { halo.ShowPrediction(false); }
                }
                break;
            case Condition.C2:
                foreach (var op in operators)
                {
                    StressHalo halo = op.GetComponentInChildren<StressHalo>();
                    op.stress_VLLThreshold = 0.2f;
                    op.stress_LMThreshold = 0.4f;
                    op.stress_MHThreshold = 0.6f;
                    op.stress_HVHThreshold = 0.8f;
                    if (halo != null) { halo.ShowPrediction(false); }
                }
                break;
            case Condition.C3:
                foreach (var op in operators)
                {
                    StressHalo halo = op.GetComponentInChildren<StressHalo>();
                    op.stress_VLLThreshold = 0.33f;
                    op.stress_LMThreshold = 0.33f;
                    op.stress_MHThreshold = 0.66f;
                    op.stress_HVHThreshold = 0.66f;
                    if (halo != null) { halo.ShowPrediction(true); }
                }
                break;
            case Condition.C4:
                foreach (var op in operators)
                {
                    StressHalo halo = op.GetComponentInChildren<StressHalo>();
                    op.stress_VLLThreshold = 0.2f;
                    op.stress_LMThreshold = 0.4f;
                    op.stress_MHThreshold = 0.6f;
                    op.stress_HVHThreshold = 0.8f;
                    if (halo != null) { halo.ShowPrediction(true); }
                }
                break;
            default: break;
        }
    }

    #endregion

    #region LiteralSymbolic
    bool Literal  = false; public bool getLiteral()  { return Literal;  }
    bool Symbolic = false; public bool getSymbolic() { return Symbolic; }

    public void ToggleLiteral() { ShowLiteral(!Literal); }
    public void ShowLiteral(bool b = true)
    {
        Literal = b;
        foreach (var panel in panels)
        {
            panel.ShowLiteral(b);
        }
    }

    public void ToggleSymbolic() { ShowSymbolic(!Symbolic); }
    public void ShowSymbolic(bool b = true)
    {
        Symbolic = b;
        foreach (var panel in panels)
        {
            panel.ShowSymbolic(b);
        }
        if(!b) { GraphManager.Graph.ResetAll(); }
    }
    #endregion

    #region Stress
    bool Stress = false; public bool getStress() { return Stress; }

    public void ToggleStress() { ShowStress(!Stress); }
    public void ShowStress(bool b)
    {
        Stress = b;
        foreach (var panel in panels)
        {
            panel.ShowStress(b);
        }
    }
    #endregion

    #region Time
    DateTime startTime;      public DateTime getStartTime()      { return startTime;      }
    DateTime lastFrame;      public DateTime getLastFrame()      { return lastFrame;      }
    TimeSpan elapsedTime;    public TimeSpan getElapsedTime()    { return elapsedTime;    }
    public float elapsedSeconds = 0.0f;

    TimeSpan taskAssignTime; public TimeSpan getTaskAssignTime() { return taskAssignTime; }
    
    void InitTime()
    {
        startTime = DateTime.UtcNow;
        taskAssignTime = TimeSpan.Zero;
    }

    void UpdateTime()
    {
        elapsedTime = System.DateTime.UtcNow - startTime;
        elapsedSeconds = (float)elapsedTime.TotalSeconds;

        if(taskAssigning) taskAssignTime += System.DateTime.UtcNow - lastFrame;
        
        lastFrame = System.DateTime.UtcNow;
    }
    #endregion

    #region ExperimentLogs
    System.Globalization.CultureInfo strFormat = System.Globalization.CultureInfo.InvariantCulture;
    string logDirectory = ""; 
    string logFile = "";
    string logFileTasks = "";

    public int checkExistingLogs()
    {
        logDirectory = Application.dataPath + "/Logs_sessions/";
        if (!File.Exists(logDirectory)) { Directory.CreateDirectory(logDirectory); }

        string[] files = Directory.GetFiles(logDirectory, "*.csv");
        int max = -1;
        for (int i = 0; i < files.Length; i++)
        {
            string[] s = files[i].Split('/');
            s = s[s.Length-1].Split('_');
            if (Convert.ToInt32(s[1]) > max) max = Convert.ToInt32(s[1]);
        }

        return max;
    }

    private bool createLogFiles()
    {
        logFile = logDirectory + "participant_" + sessionID.ToString() + "_trial_" + trialID.ToString() + "_condition_" + condition.String() + "_OpStatus.csv";
        if (File.Exists(logFile))
        {
            Debug.LogError(("Logfile for session " + sessionID
                        + ", trial " + trialID
                        + " and condition " + condition.String() + " already exists !"));
            return false;
        }

        logFileTasks = logDirectory + "participant_" + sessionID.ToString() + "_trial_" + trialID.ToString() + "_condition_" + condition.String() + "_TskAssignment.csv";
        if (File.Exists(logFileTasks))
        {
            Debug.LogError(("Logfile for task assignment of session " + sessionID
                        + ", trial " + trialID
                        + " and condition " + condition.String() + " already exists !"));
            return false;
        }

        return true;
    }

    private int InitLogs()
    {
        checkExistingLogs();
        
        if(sessionID < 0) { Debug.LogError("Please select a session ID"); return -1; }
        if(trialID < 0)   { Debug.LogError("Please select a trial ID");   return -1; }

        if(!createLogFiles()) return -2;

        string header = "Time (s),";
        header += "Time spent assigning tasks (s),";
        for (int i = 0; i < 6; i++)
        {
            header += "Op" + i.ToString() + " Speed,"
                    + "Op" + i.ToString() + " Stress value (%),"
                    + "Op" + i.ToString() + " Stress level,"
                    + "Op" + i.ToString() + " Portion of time worked (%),"
                    + "Op" + i.ToString() + " Task to do,"
                    + "Op" + i.ToString() + " Task done,"
                    + "Op" + i.ToString() + " Success,"
                    + "Op" + i.ToString() + " Failure,"
                    + "Op" + i.ToString() + " SuccessRate (%),"
                    + "Op" + i.ToString() + " Score,"
                    + "Op" + i.ToString() + " Average score per task (%),";
        }
        File.AppendAllText(logFile, header + "\n");

        File.AppendAllText(logFileTasks, "Time (s),Event type,Data\n");

        return 0;
    }

    void UpdateLogs(bool lastFrame = false)
    {
        string line = elapsedTime.TotalSeconds.ToString("###0.000','", strFormat); //Time
        line += taskAssignTime.TotalSeconds.ToString("###0.000','", strFormat);    // Time spent assigning tasks
        for (int i = 0; i < 6; i++)
        {
            line += operators[i].speed.ToString("##0.00','", strFormat)            //Speed
                  + operators[i].stress.ToString("##0.00','", strFormat)           //Stress value
                  + operators[i].stressLevel.String() + ','                        //Stress level
                  + operators[i].timeWorkedRatio.ToString("##0.00','", strFormat)  //Portion of time worked
                  + operators[i].taskTodoCount().ToString() + ','                  //Task to do
                  + operators[i].taskDoneCount().ToString() + ','                  //Task done
                  + operators[i].getSuccess().ToString() + ','                     //Success
                  + operators[i].getFailure() + ','                                //Failure
                  + operators[i].getSuccessRate().ToString("##0.00','", strFormat) //SuccessRate
                  + operators[i].getScore().ToString("##0.00','", strFormat)       //Score
                  + operators[i].getScoreAvg().ToString("##0.00','", strFormat);   //Average score per task
        }
        File.AppendAllText(logFile, line + "\n");

        if (lastFrame)
        {
            string endOfTrial = "End of Trial\n";
            File.AppendAllText(logFile, endOfTrial);
            File.AppendAllText(logFileTasks, endOfTrial);
        }
    }

    public void LogTaskCreated(int count)
    {
        if (scenario != ScenarioType.Trial) return;
        string line = elapsedTime.TotalSeconds.ToString("###0.000','", strFormat) + "Task Created," + count.ToString() + '\n';
        File.AppendAllText(logFileTasks, line);
    }

    public void LogTaskMissed(MATBIISystem.MATBII_TASK task)
    {
        if (scenario != ScenarioType.Trial) return;
        string line = elapsedTime.TotalSeconds.ToString("###0.000','", strFormat) + "Task Missed," + MATBIISystem.MATBII_TASK_ToString(task) + "\n";
        File.AppendAllText(logFileTasks, line);
    }

    public void LogTaskAssigned(Operator op, MATBIISystem.MATBII_TASK task)
    {
        if (scenario != ScenarioType.Trial) return;
        string line = elapsedTime.TotalSeconds.ToString("###0.000','", strFormat) + "Task Assigned," + "Op" + op.ID + ": " + MATBIISystem.MATBII_TASK_ToString(task) + "\n";
        File.AppendAllText(logFileTasks, line);
    }
    #endregion

    #region ExperimentControls
    [SerializeField] bool experimentRunning = false; public bool isRunning() { return experimentRunning; }
    [SerializeField] TaskSchedule OneBatch;
    [SerializeField] TaskSchedule InitialTraining;
    [SerializeField] TaskSchedule TrialTraining;
    [SerializeField] TaskSchedule Trial;

    public void StartTrial()
    {
        Interrupt();
        ResetOperators();

        List<int> opIndexes = new List<int>(){0,1,2,3,4,5};
        
        // Randomize slow & fast operators placement
        opIndexes.Shuffle();
        // 2 slow operators
        operators[opIndexes[0]].speed = 0.75f;
        operators[opIndexes[1]].speed = 0.75f;
        // 2 normal speed operators
        operators[opIndexes[2]].speed = 1.00f;
        operators[opIndexes[3]].speed = 1.00f;
        // 2 fast operators
        operators[opIndexes[4]].speed = 1.50f;
        operators[opIndexes[5]].speed = 1.50f;

        // Randmomize distribution of initial stress
        opIndexes.Shuffle();
        // 2 not stressed operators
        operators[opIndexes[0]].stress = 0f;
        operators[opIndexes[1]].stress = 0f;
        // 2 LOW stressed operators
        operators[opIndexes[2]].stress = 0.3f;
        operators[opIndexes[3]].stress = 0.3f;
        // 2 MEDIUM stressed operators
        operators[opIndexes[4]].stress = 0.5f;
        operators[opIndexes[5]].stress = 0.5f;

        // Randmomize distribution of initial tasks
        opIndexes.Shuffle();
        // 2 operators with 1 task
        operators[opIndexes[0]].GiveRandomTask(1);
        operators[opIndexes[1]].GiveRandomTask(1);
        // 2 operators with 2 tasks
        operators[opIndexes[2]].GiveRandomTask(2);
        operators[opIndexes[3]].GiveRandomTask(2);
        // 2 operators with 3 tasks
        operators[opIndexes[4]].GiveRandomTask(3);
        operators[opIndexes[5]].GiveRandomTask(3);

        // Assign a task schedule to the spawner
        switch (scenario)
        {
            case ExperimentControlsMATBIITeam.ScenarioType.OneBatch:
                spawner.schedule = OneBatch; break;
            case ExperimentControlsMATBIITeam.ScenarioType.InitialTraining:
                spawner.schedule = InitialTraining; break;
            case ExperimentControlsMATBIITeam.ScenarioType.TrialTraining:
                spawner.schedule = TrialTraining; break;
            case ExperimentControlsMATBIITeam.ScenarioType.Trial:
                spawner.schedule = Trial; break;
            case ExperimentControlsMATBIITeam.ScenarioType.Infinite:
            default: spawner.schedule = null; break;
        }

        if (scenario == ScenarioType.Trial) { if (InitLogs() < 0) { return; } }
        
        InitTime();
        spawner.StartSchedule();
        experimentRunning = true;
        Debug.Log("ExperimentControls: Scenario Started");
    }

    // General stop for everything
    public void Interrupt()
    {
        experimentRunning = false;

        spawner.ResetSpawner();
        for (int c = 0; c < spawner.transform.childCount; c++) { Destroy(spawner.transform.GetChild(c).gameObject); }

        foreach (var op in operators) { op.ResetTaskQueue(); op.replayer.Stop(); }
    }

    public void ResetOperators()
    {
        foreach (var op in operators)
        {
            op.ResetStress();
            op.ResetTaskQueue();
            op.ResetTaskHistory();
            op.ResetWorkingTime();
        }
    }

    bool Finished()
    {
        //check that all task from the schedule have been spawned
        if (spawner.started) return false;

        //check that all task spawned have been assigned
        if (spawner.transform.childCount > 0) return false;

        //check that all operators have finished their task
        foreach (var op in operators) { if (op.taskTodoCount() > 0) return false; }
        
        Debug.Log("ExperimentControls: Finished");
        return true;
    }
    #endregion

    #region MonoBehaviourCallbacks
    void Start()
    {
        Bind();
        //ShowLiteral(false);
        //ShowSymbolic(false);
        //ShowStress(false);
    }

    void FixedUpdate()
    {
        if (!experimentRunning) return;

        bool lastFrame = Finished();
        UpdateTime();
        if (scenario == ScenarioType.Trial) { UpdateLogs(lastFrame); }
        if (lastFrame) Interrupt();
    }
    #endregion

    #region Interaction
    private bool leftHand = false; public bool LeftHand() { return leftHand; }
    private MATBIISystem.MATBII_TASK leftTask = MATBIISystem.MATBII_TASK.SYSMON;
    public MATBIISystem.MATBII_TASK LeftTask() { return leftTask; }
    public bool setLeftHand(bool b, MATBIISystem.MATBII_TASK task)
    {
        leftHand = b;
        leftTask = task;
        return taskAssigning;
    }
    private bool rightHand = false; public bool RightHand() { return rightHand; }
    private MATBIISystem.MATBII_TASK rightTask = MATBIISystem.MATBII_TASK.SYSMON;
    public MATBIISystem.MATBII_TASK RightTask() { return rightTask; }
    public bool setRightHand(bool b, MATBIISystem.MATBII_TASK task)
    {
        rightHand = b;
        rightTask = task;
        return taskAssigning;
    }
    public bool taskAssigning {get {return leftHand || rightHand;}}
    #endregion

    public float getTeamScore()
    {
        float score = 0f;
        for (int i = 0; i < 6; i++)
        {
            score += operators[i].getScore();
        }
        return score;
    }
    public float getMaxScore()
    {
        float score = 0f;
        for (int i = 0; i < 6; i++)
        {
            score += operators[i].getMaxScore();
        }
        return score;
    }
}

public static class RandomExtension
{
    private static System.Random rng = new System.Random();  

    public static void Shuffle<T>(this IList<T> list)  
    {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ExperimentControlsMATBIITeam)), CanEditMultipleObjects]
public class ExperimentControlsMATBIITeamEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ExperimentControlsMATBIITeam script = (ExperimentControlsMATBIITeam)target;

        if(Application.isPlaying && script.isRunning() && script.scenario != ExperimentControlsMATBIITeam.ScenarioType.Infinite)
        {
            int index = script.getSpawner().index;
            int count = script.getSpawner().schedule.IncomingTasks.Count;
            GUILayout.Label("Running: " + (script.getSpawner().started ? index.ToString("##0") : count.ToString()) + "/" + count.ToString() + " batches delivered.");
        }

        int higherLogID = script.checkExistingLogs();
        GUILayout.Label(higherLogID > -1 ? "Found logs for sessions up to: " + higherLogID.ToString() : "No logs found");

        /*
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("No Cue" + (!script.getLiteral() && !script.getSymbolic() && !script.getStress() ? " (active)" : "")))
        {
            script.ShowSymbolic(false);
            script.ShowLiteral(false);
            script.ShowStress(false);
        }
        if(GUILayout.Button("Hybrid" + (script.getLiteral() && script.getSymbolic() ? " (active)" : "")))
        {
            script.ShowSymbolic(true);
            script.ShowLiteral(true);
        }
        GUILayout.EndHorizontal();
        */

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Literal" + (script.getLiteral() && !script.getSymbolic() ? " (active)" : "")))
        {
            script.ShowLiteral(true);
            script.ShowSymbolic(false);
        }
        if(GUILayout.Button("Symbolic" + (!script.getLiteral() && script.getSymbolic() ? " (active)" : "")))
        {
            script.ShowSymbolic(true);
            script.ShowLiteral(false);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("No stress fb" + (script.getStress() ? "" : " (active)")))
        {
            script.ShowStress(false);
        }
        if(GUILayout.Button("Stress fb" + (script.getStress() ? " (active)" : "")))
        {
            script.ShowStress(true);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
                if(GUILayout.Button("C0"))
                {
                    script.changeCondition(ExperimentControlsMATBIITeam.Condition.C0);
                }
                GUILayout.Label("3 Colors");
                GUILayout.Label("5 Colors");
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
                GUILayout.Label("Without Prediction");
                if(GUILayout.Button("C1" + (script.condition == ExperimentControlsMATBIITeam.Condition.C1 ? " (active)" : "")))
                {
                    script.changeCondition(ExperimentControlsMATBIITeam.Condition.C1);
                }
                if(GUILayout.Button("C2" + (script.condition == ExperimentControlsMATBIITeam.Condition.C2 ? " (active)" : "")))
                {
                    script.changeCondition(ExperimentControlsMATBIITeam.Condition.C2);
                }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
                GUILayout.Label("With Prediction");
                if(GUILayout.Button("C3" + (script.condition == ExperimentControlsMATBIITeam.Condition.C3 ? " (active)" : "")))
                {
                    script.changeCondition(ExperimentControlsMATBIITeam.Condition.C3);
                }
                if(GUILayout.Button("C4" + (script.condition == ExperimentControlsMATBIITeam.Condition.C4 ? " (active)" : "")))
                {
                    script.changeCondition(ExperimentControlsMATBIITeam.Condition.C4);
                }
            GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Trial / One batch"))
        {
            if (script.scenario != ExperimentControlsMATBIITeam.ScenarioType.Trial)
            {
                script.scenario = ExperimentControlsMATBIITeam.ScenarioType.Trial;
            }
            else
            {
                script.scenario = ExperimentControlsMATBIITeam.ScenarioType.OneBatch;
            }
        }

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Start"))
        {
            script.StartTrial();
        }
        if(GUILayout.Button("Stop"))
        {
            if (script.isRunning())
            {
                script.Interrupt();
            }
            else
            {
                script.ResetOperators();
            }
        }
        GUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
#endif
