using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRUIButton : Interactive
{
    [Header("Events")]
    [SerializeField] protected UnityEvent clicked;
    
    private bool pressing = false;
    public bool isPressed() { return pressing; }

    public Outline outline;

    // Start is called before the first frame update
    protected void Start()
    {
        if (clicked == null)
        {
            clicked = new UnityEvent();
            clicked.AddListener(()=>{print("No event on " + gameObject.name);});
        }
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    private void OnEnable()
    {
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
        pressing = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            pressing = true;
            if (outline != null) outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            pressing = false;
            if (outline != null) outline.enabled = false;
        }
    }

    override public void Click()
    {
        if (!pressing) return;
        clicked.Invoke();
    }

    override public void Release()
    {
        // ...
    }
}
