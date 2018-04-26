using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PseudoGravityBody : MonoBehaviour {
    public PseudoGravityAttractor attractor;
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
