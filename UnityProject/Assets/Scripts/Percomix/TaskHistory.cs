using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskHistory : MonoBehaviour
{
    [SerializeField] Operator op;

    [Header("Tasks")]

    [SerializeField] TextMeshPro SYSMON_count;
    [SerializeField] Transform SYSMON_cubes = null;
    [SerializeField] GameObject SYSMON_cubePrefab = null;

    [SerializeField] TextMeshPro TRACK_count;
    [SerializeField] Transform TRACK_cubes = null;
    [SerializeField] GameObject TRACK_cubePrefab = null;

    [SerializeField] TextMeshPro COMM_count;
    [SerializeField] Transform COMM_cubes = null;
    [SerializeField] GameObject COMM_cubePrefab = null;

    [SerializeField] TextMeshPro RESMAN_count;
    [SerializeField] Transform RESMAN_cubes = null;
    [SerializeField] GameObject RESMAN_cubePrefab = null;

    [Header("Portion of time working")]
    
    [SerializeField] TextMeshPro WorkingTime_label = null;
    [SerializeField] Image WorkingTime_imageToFill = null;

    [Header("Stats")]

    [SerializeField] TextMeshPro Success_count = null;

    [SerializeField] TextMeshPro Failure_count = null; 
    
    [SerializeField] TextMeshPro SuccessRate_count = null; 
    [SerializeField] Image SuccessRate_imageToFill = null; 
    
    [SerializeField] TextMeshPro Score_count = null; 
    
    [SerializeField] TextMeshPro ScoreAvg_count = null; 
    [SerializeField] Image ScoreAvg_imageToFill = null; 

    void Update()
    {
        SYSMON_count.text = op.SYSMON_done.ToString();
        if (SYSMON_cubes.childCount < op.SYSMON_done)
        {
            GameObject go = Instantiate(SYSMON_cubePrefab);
            go.transform.SetParent(SYSMON_cubes);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(0, -1.2f * (SYSMON_cubes.childCount -1), 0);
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(SYSMON_cubes.gameObject.activeSelf);
        }
        if (SYSMON_cubes.childCount > op.SYSMON_done)
        {
            Destroy(SYSMON_cubes.GetChild(SYSMON_cubes.childCount -1).gameObject);
        }

        TRACK_count.text = op.TRACK_done.ToString();
        if (TRACK_cubes.childCount < op.TRACK_done)
        {
            GameObject go = Instantiate(TRACK_cubePrefab);
            go.transform.SetParent(TRACK_cubes);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(0, -1.2f * (TRACK_cubes.childCount -1), 0);
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(TRACK_cubes.gameObject.activeSelf);
        }
        if (TRACK_cubes.childCount > op.TRACK_done)
        {
            Destroy(TRACK_cubes.GetChild(TRACK_cubes.childCount -1).gameObject);
        }

        COMM_count.text = op.COMM_done.ToString();
        if (COMM_cubes.childCount < op.COMM_done)
        {
            GameObject go = Instantiate(COMM_cubePrefab);
            go.transform.SetParent(COMM_cubes);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(0, -1.2f * (COMM_cubes.childCount -1), 0);
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(COMM_cubes.gameObject.activeSelf);
        }
        if (COMM_cubes.childCount > op.COMM_done)
        {
            Destroy(COMM_cubes.GetChild(COMM_cubes.childCount -1).gameObject);
        }

        RESMAN_count.text = op.RESMAN_done.ToString();
        if (RESMAN_cubes.childCount < op.RESMAN_done)
        {
            GameObject go = Instantiate(RESMAN_cubePrefab);
            go.transform.SetParent(RESMAN_cubes);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = new Vector3(0, -1.2f * (RESMAN_cubes.childCount -1), 0);
            go.transform.localRotation = Quaternion.identity;
            go.SetActive(RESMAN_cubes.gameObject.activeSelf);
        }
        if (RESMAN_cubes.childCount > op.RESMAN_done)
        {
            Destroy(RESMAN_cubes.GetChild(RESMAN_cubes.childCount -1).gameObject);
        }

        WorkingTime_label.text = (op.timeWorkedRatio * 100).ToString("#0#'%'");
        WorkingTime_imageToFill.fillAmount = op.timeWorkedRatio;
        
        Success_count.text = op.getSuccess().ToString();

        Failure_count.text = op.getFailure().ToString();

        SuccessRate_count.text = (op.getSuccessRate() * 100.0).ToString("##0'%'");
        SuccessRate_imageToFill.fillAmount = op.getSuccessRate();

        Score_count.text = op.getScore().ToString("##0.0");

        ScoreAvg_count.text = (op.getScoreAvg() * 100.0).ToString("##0'%'");
        ScoreAvg_imageToFill.fillAmount = op.getScoreAvg();
    }
}
