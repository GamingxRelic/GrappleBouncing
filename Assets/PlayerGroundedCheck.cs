using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedCheck : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private int collisionCount = 0;

    void Update()
    {
        playerController.is_grounded = collisionCount > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        collisionCount++;
    }

    void OnTriggerExit(Collider other)
    {
        collisionCount--;

        
    }
}
