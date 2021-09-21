using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRUIButton : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] UnityEvent clicked;

    // Start is called before the first frame update
    void Start()
    {
        if (clicked == null) clicked = new UnityEvent();
        //clicked.AddListener(Ping);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            clicked.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        //...
    }

}
