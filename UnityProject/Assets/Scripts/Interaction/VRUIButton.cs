using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRUIButton : Interactive
{
    [Header("Events")]
    [SerializeField] protected UnityEvent clicked;
    
    private int pressing = 0;
    public bool isPressed() { return pressing > 0; }

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
        pressing = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            pressing++;
            if (outline != null) outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            pressing--;
            if (outline != null) outline.enabled = isPressed();
        }
    }

    override public void Click()
    {
        if (!isPressed()) return;
        clicked.Invoke();
    }

    override public void Release()
    {
        // ...
    }
}
