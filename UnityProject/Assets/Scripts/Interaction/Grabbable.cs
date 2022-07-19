using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbable : Interactive
{
    private Outline outline;
    private Rigidbody rBody;
    private Transform interactor = null;

    void Start()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
        rBody = GetComponent<Rigidbody>();
    }

    override public void Click()
    {
        print("CLICK");
        this.transform.SetParent(interactor, true);
        rBody.useGravity = false;
        rBody.isKinematic = true;
    }

    override public void Release()
    {
        print("RELEASE");
        this.transform.SetParent(null, true);
        rBody.useGravity = true;
        rBody.isKinematic = false;
    }

    public void OnCollisionEnter(Collision col)
    {
        Collider other = col.collider;
        if (other.tag == "LeftHand" || other.tag == "RightHand"  || other.tag == "Cursor")
        {
            interactor = other.transform;
            outline.enabled = true;
        }
    }

    public void OnCollisionExit(Collision col)
    {
        Collider other = col.collider;
        if (other.tag == "LeftHand" || other.tag == "RightHand"  || other.tag == "Cursor")
        {
            outline.enabled = false;
        }
    }

    /*
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand"  || other.tag == "Cursor")
        {
            interactor = other.transform;
            outline.enabled = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand"  || other.tag == "Cursor")
        {
            outline.enabled = false;
        }
    }
    */
}
