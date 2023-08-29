using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskQueueDisplay : MonoBehaviour
{
    [SerializeField] Operator op;

    [SerializeField] GameObject RestPanel;

    [Header("SYSMON")]
    [SerializeField] GameObject SYSMON_root = null;
    [SerializeField] TextMeshPro SYSMON_count = null;
    [SerializeField] TextMeshPro SYSMON_progress = null;
    [SerializeField] Image SYSMON_imageToFill = null;
    [SerializeField] Image SYSMON_imageBG = null;
    [SerializeField] Transform SYSMON_cubes = null;
    [SerializeField] GameObject SYSMON_cubePrefab = null;

    [Header("TRACK")]
    [SerializeField] GameObject TRACK_root = null;
    [SerializeField] TextMeshPro TRACK_count = null;
    [SerializeField] TextMeshPro TRACK_progress = null;
    [SerializeField] Image TRACK_imageToFill = null;
    [SerializeField] Image TRACK_imageBG = null;
    [SerializeField] Transform TRACK_cubes = null;
    [SerializeField] GameObject TRACK_cubePrefab = null;

    [Header("COMM")]
    [SerializeField] GameObject COMM_root = null;
    [SerializeField] TextMeshPro COMM_count = null;
    [SerializeField] TextMeshPro COMM_progress = null;
    [SerializeField] Image COMM_imageToFill = null;
    [SerializeField] Image COMM_imageBG = null;
    [SerializeField] Transform COMM_cubes = null;
    [SerializeField] GameObject COMM_cubePrefab = null;

    [Header("RESMAN")]
    [SerializeField] GameObject RESMAN_root = null;
    [SerializeField] TextMeshPro RESMAN_count = null;
    [SerializeField] TextMeshPro RESMAN_progress = null;
    [SerializeField] Image RESMAN_imageToFill = null;
    [SerializeField] Image RESMAN_imageBG = null;
    [SerializeField] Transform RESMAN_cubes = null;
    [SerializeField] GameObject RESMAN_cubePrefab = null;

    [Header("All tasks")]
    [SerializeField] GameObject AllTasks_root = null;
    [SerializeField] TextMeshPro AllTasks_progress = null;
    [SerializeField] Image AllTasks_imageToFill = null;

    void Start()
    {
        SYSMON_root.SetActive(false);
        TRACK_root.SetActive(false);
        COMM_root.SetActive(false);
        RESMAN_root.SetActive(false);
        AllTasks_root.SetActive(false);
    }

    void Update()
    {
        if (op.SYSMON_todo > 0 && !SYSMON_root.activeSelf) { SYSMON_root.SetActive(true); AllTasks_root.SetActive(true);}
        if (op.TRACK_todo  > 0 && !TRACK_root.activeSelf)  { TRACK_root.SetActive(true);  AllTasks_root.SetActive(true);}
        if (op.COMM_todo   > 0 && !COMM_root.activeSelf)   { COMM_root.SetActive(true);   AllTasks_root.SetActive(true);}
        if (op.RESMAN_todo > 0 && !RESMAN_root.activeSelf) { RESMAN_root.SetActive(true); AllTasks_root.SetActive(true);}
        
        if (op.SYSMON_todo <= 0 && SYSMON_root.activeSelf) SYSMON_root.SetActive(false);
        if (op.TRACK_todo  <= 0 && TRACK_root.activeSelf)  TRACK_root.SetActive(false); 
        if (op.COMM_todo   <= 0 && COMM_root.activeSelf)   COMM_root.SetActive(false); 
        if (op.RESMAN_todo <= 0 && RESMAN_root.activeSelf) RESMAN_root.SetActive(false);

        if (op.SYSMON_todo <= 0 && op.TRACK_todo <= 0 && op.COMM_todo <= 0 && op.RESMAN_todo <= 0)
        {
            if (AllTasks_root.activeSelf) AllTasks_root.SetActive(false);
            RestPanel.SetActive(true);
        }
        else RestPanel.SetActive(false);

        if (SYSMON_root.activeSelf)
        {
            SYSMON_count.text = op.SYSMON_todo.ToString();
            ///*
            if (SYSMON_cubes.childCount < op.SYSMON_todo)
            {
                GameObject go = Instantiate(SYSMON_cubePrefab);
                go.transform.SetParent(SYSMON_cubes);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(0, -1.2f * (SYSMON_cubes.childCount -1), 0);
                go.transform.localRotation = Quaternion.identity;
                go.SetActive(SYSMON_cubes.gameObject.activeSelf);
            }
            if (SYSMON_cubes.childCount > op.SYSMON_todo)
            {
                Destroy(SYSMON_cubes.GetChild(SYSMON_cubes.childCount -1).gameObject);
            }
            //*/

            if (op.replayer.TASK == MATBIISystem.MATBII_TASK.SYSMON)
            {
                float progress = op.replayer.getProgress();
                SYSMON_progress.text =  (progress * 100).ToString("##0'%'");
                SYSMON_imageToFill.fillAmount = progress;
                SYSMON_imageBG.fillAmount = 1;
            }
            else
            {
                SYSMON_progress.text = "";
                SYSMON_imageToFill.fillAmount = 0;
                SYSMON_imageBG.fillAmount = 0;
            }
        }
        
        if (TRACK_root.activeSelf)
        {
            TRACK_count.text = op.TRACK_todo.ToString();
            ///*
            if (TRACK_cubes.childCount < op.TRACK_todo)
            {
                GameObject go = Instantiate(TRACK_cubePrefab);
                go.transform.SetParent(TRACK_cubes);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(0, -1.2f * (TRACK_cubes.childCount -1), 0);
                go.transform.localRotation = Quaternion.identity;
                go.SetActive(TRACK_cubes.gameObject.activeSelf);
            }
            if (TRACK_cubes.childCount > op.TRACK_todo)
            {
                Destroy(TRACK_cubes.GetChild(TRACK_cubes.childCount -1).gameObject);
            }
            //*/

            if (op.replayer.TASK == MATBIISystem.MATBII_TASK.TRACK)
            {
                float progress = op.replayer.getProgress();
                TRACK_progress.text =  (progress * 100).ToString("##0'%'");
                TRACK_imageToFill.fillAmount = progress;
                TRACK_imageBG.fillAmount = 1;
            }
            else
            {
                TRACK_progress.text = "";
                TRACK_imageToFill.fillAmount = 0;
                TRACK_imageBG.fillAmount = 0;
            }
        }

        if (COMM_root.activeSelf)
        {
            COMM_count.text = op.COMM_todo.ToString();
            ///*
            if (COMM_cubes.childCount < op.COMM_todo)
            {
                GameObject go = Instantiate(COMM_cubePrefab);
                go.transform.SetParent(COMM_cubes);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(0, -1.2f * (COMM_cubes.childCount -1), 0);
                go.transform.localRotation = Quaternion.identity;
                go.SetActive(COMM_cubes.gameObject.activeSelf);
            }
            if (COMM_cubes.childCount > op.COMM_todo)
            {
                Destroy(COMM_cubes.GetChild(COMM_cubes.childCount -1).gameObject);
            }
            //*/

            if (op.replayer.TASK == MATBIISystem.MATBII_TASK.COMM)
            {
                float progress = op.replayer.getProgress();
                COMM_progress.text =  (progress * 100).ToString("##0'%'");
                COMM_imageToFill.fillAmount = progress;
                COMM_imageBG.fillAmount = 1;
            }
            else
            {
                COMM_progress.text = "";
                COMM_imageToFill.fillAmount = 0;
                COMM_imageBG.fillAmount = 0;
            }
        }
        
        if (RESMAN_root.activeSelf)
        {
            RESMAN_count.text = op.RESMAN_todo.ToString();
            ///*
            if (RESMAN_cubes.childCount < op.RESMAN_todo)
            {
                GameObject go = Instantiate(RESMAN_cubePrefab);
                go.transform.SetParent(RESMAN_cubes);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(0, -1.2f * (RESMAN_cubes.childCount -1), 0);
                go.transform.localRotation = Quaternion.identity;
                go.SetActive(RESMAN_cubes.gameObject.activeSelf);
            }
            if (RESMAN_cubes.childCount > op.RESMAN_todo)
            {
                Destroy(RESMAN_cubes.GetChild(RESMAN_cubes.childCount -1).gameObject);
            }
            //*/

            if (op.replayer.TASK == MATBIISystem.MATBII_TASK.RESMAN)
            {
                float progress = op.replayer.getProgress();
                RESMAN_progress.text =  (progress * 100).ToString("##0'%'");
                RESMAN_imageToFill.fillAmount = progress;
                RESMAN_imageBG.fillAmount = 1;
            }
            else
            {
                RESMAN_progress.text = "";
                RESMAN_imageToFill.fillAmount = 0;
                RESMAN_imageBG.fillAmount = 0;
            }
        }

        if (AllTasks_root.activeSelf)
        {
            float progress = op.getProgress();
            AllTasks_imageToFill.fillAmount = progress;
            AllTasks_progress.text = (progress * 100).ToString("##0'%'");
        }
    }
}
