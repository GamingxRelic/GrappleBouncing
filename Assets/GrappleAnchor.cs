using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class GrappleAnchor : MonoBehaviour
{
    public GrapplingHook grapple;
    [SerializeField] private Rigidbody rb;

    public void Launch(Vector3 force) {
        rb.AddForce(force, ForceMode.Impulse);
    }
    void OnTriggerEnter(Collider other)
    {   
        grapple.SetGrappleAnchorPointAndStartGrapple(transform.position);
        Destroy(gameObject);
    }
}
