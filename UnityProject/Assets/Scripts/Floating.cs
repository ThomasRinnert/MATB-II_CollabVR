using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floating : MonoBehaviour
{
    public float rotationSpeed = 20.0f;
    public float floating = 0.02f;

    private Vector3 origin = new Vector3();
    private float t = 0;

    void Start()
    {
        origin = transform.localPosition;
    }

    void Update()
    {
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.Self);
        t = (t + Time.deltaTime) % (2.0f * Mathf.PI);
        transform.localPosition = new Vector3(origin.x, origin.y + floating * Mathf.Sin(t), origin.z);
    }
}
