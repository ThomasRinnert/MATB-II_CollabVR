using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StressOverTimeGraph : MonoBehaviour
{
    public string graphID = "";

    private Operator op;
    private Image MHThresholdBar = null;
    private Image LMThresholdBar = null;

    public List<Color> colors = new List<Color>(5){Color.red, /*orange:*/ new Color(1.0f, 0.64f, 0.0f), Color.yellow, Color.green, Color.blue};

    private Color color = Color.gray;
    private int count = 0;

    void Start()
    {
        op = GetComponentInParent<Operator>();
        MHThresholdBar = transform.GetChild(0).GetComponent<Image>();
        LMThresholdBar = transform.GetChild(1).GetComponent<Image>();
    }

    public void ResetGraph()
    {
        if (GraphManager.Graph == null) return;
        if (GraphManager.Graph.Graphs == null) return;
        GraphManager.GPUGraphData g = GraphManager.Graph.Graphs[graphID];
        if (g != null) g.Reset();
    }

    void Update()
    {
        if (op.control == null) return;
        if (!op.control.isRunning()) return;
        if(Camera.main.gameObject.GetComponent<GraphManager>() == null)
        {
            Camera.main.gameObject.AddComponent<GraphManager>();
        }
        if(LMThresholdBar.rectTransform.localPosition.y != op.stress_LMThreshold * 100.0f - 50.0f)
        {
            LMThresholdBar.rectTransform.localPosition = new Vector3(LMThresholdBar.rectTransform.localPosition.x,
                                                                    op.stress_LMThreshold * 100.0f - 50.0f,
                                                                    LMThresholdBar.rectTransform.localPosition.z);
        }
        if(MHThresholdBar.rectTransform.localPosition.y != op.stress_MHThreshold * 100.0f - 50.0f)
        {
            MHThresholdBar.rectTransform.localPosition = new Vector3(MHThresholdBar.rectTransform.localPosition.x,
                                                                    op.stress_MHThreshold * 100.0f - 50.0f,
                                                                    MHThresholdBar.rectTransform.localPosition.z);
        }
    }

    void FixedUpdate()
    {
        if (op.control == null) return;
        if (!op.control.isRunning()) return;
        count++;
        if(GraphManager.Graph != null && count > 10) // aproximatively every .21s
        {
            count = 0;
            graphID = "Stress_Over_Time_" + op.ID;

            if(op.stress_MHThreshold < op.stress && color != colors[1]) color = colors[1];
            if(op.stress_LMThreshold < op.stress && op.stress < op.stress_MHThreshold && color != colors[2]) color = colors[2];
            if(op.stress < op.stress_LMThreshold && color != colors[4]) color = colors[4];

            GraphManager.Graph.Plot(graphID, op.stress, color, new GraphManager.Matrix4x4Wrapper(transform.position, transform.rotation, transform.localScale));
            
            GraphManager.GPUGraphData g = GraphManager.Graph.Graphs[graphID];
            if (g != null)
            {
                if (g.Range == null) g.Range = new float[] {0.0f, 1.0f};
                if (g.MaxNumPoints != 1000) g.MaxNumPoints = 1000; // 3.5 last mins displayed
                if (g.MinUI.activeSelf) g.MinUI.SetActive(false);
                if (g.MaxUI.activeSelf) g.MaxUI.SetActive(false);
                if (g.AvgUI.activeSelf) g.AvgUI.SetActive(false);
            }
        }
    }
}
