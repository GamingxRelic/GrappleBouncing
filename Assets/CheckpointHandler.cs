using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointHandler : MonoBehaviour
{
    public static Transform current_checkpoint;
    [SerializeField] private Transform initial_transform;

    void Start()
    {
        current_checkpoint = initial_transform;
    }

    public void Teleport() {
        gameObject.transform.position = current_checkpoint.position;
    }
}
