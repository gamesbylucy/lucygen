using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PGBody : MonoBehaviour {
    public PGAttractor attractor;
    private Transform myTransform;
    Rigidbody myRigidbody;

    private void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();
        myRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        myRigidbody.useGravity = false;
        myTransform = transform;
    }

    private void FixedUpdate()
    {
        attractor.attract(myTransform);
    }
}
