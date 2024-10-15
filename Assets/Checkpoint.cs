using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player")) {
            CheckpointHandler.current_checkpoint.position = transform.position;
        }
    }
}
