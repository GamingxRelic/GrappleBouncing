using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathCube : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")) {
            other.gameObject.GetComponent<PlayerController>().Die();
        }   
    }
}
