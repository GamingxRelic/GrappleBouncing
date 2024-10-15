using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.EventSystems;

public class GrapplingHook : MonoBehaviour
{
    [Header("Grapple Components")]
    [SerializeField] private PlayerController player;

    [Header("Grapple Variables")]
    [SerializeField] public bool is_grappling = false;
    [SerializeField] private float grapple_speed = 20f;
    [SerializeField] private float spring_constant = 26f;
    [SerializeField] private float grappling_max_speed = 30f;
    [SerializeField] private float grappling_max_fall_speed = 10f;
    [SerializeField] private float grapple_min_length = 3f;
    [SerializeField] private float grappling_max_distance = 500f;
    [SerializeField] private float grappling_gravity_scale = 1f;
    [SerializeField] private float impulse_strength = 10f;
    [SerializeField] private float impulse_up_strength = 10f;

    [Header("Anchor Variables")]
    [SerializeField] private Transform anchor_launch_point;
    [SerializeField] LayerMask whatIsGround;

    [Header("Internal Variables")]
    private Vector3 point_a, point_b;
    private Vector3 move_direction;
    private float initial_length;
    private LineRenderer line;


    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();    
        line.startWidth = 0.2f;
        line.endWidth = 0.2f;
        line.startColor = Color.red;
        line.endColor = Color.red;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            RaycastFromCamera();
        } 
        else if(Input.GetMouseButtonUp(0)) {
            StopGrapple();
        }

        if(is_grappling) {
            // Draw the grapple line
            line.SetPosition(0, point_a);
            line.SetPosition(1, point_b);
        }
    }

    private void FixedUpdate()
    {
        if(is_grappling) {
            GrappleMovement();
        }
    }

    private void StartGrapple() {
        is_grappling = true;
        line.positionCount = 2;
        initial_length = Vector3.Distance(point_a, point_b);

        // Player Speed Overrides
        player.max_speed_override = grappling_max_speed;
        player.max_fall_speed_override = grappling_max_fall_speed;
        player.gravity_scale_override = grappling_gravity_scale;
    }

    private void StopGrapple() {
        if(!is_grappling) return;

        is_grappling = false;
        line.positionCount = 0;

        // Add an impulse at the end of the swing
        float distance_to_point = Vector3.Distance(point_a, point_b);
        
        Vector3 impulse = player.transform.up * impulse_up_strength/distance_to_point + player.rb.velocity.normalized * impulse_strength/distance_to_point; 
        player.rb.AddForce(impulse, ForceMode.Impulse);

        player.max_speed_override = 0f;
        player.max_fall_speed_override = 0f;
        player.gravity_scale_override = 0f;
    }

    public void StopGrapple(bool forced) {
        if(!is_grappling) return;

        is_grappling = false;
        line.positionCount = 0;

        player.max_speed_override = 0f;
        player.max_fall_speed_override = 0f;
        player.gravity_scale_override = 0f;
    }

    private void GrappleMovement() {
        point_a = player.transform.position;
        move_direction = point_a - point_b;
        move_direction.Normalize();

        float distance_to_point = Vector3.Distance(point_a, point_b);

        float k = spring_constant * player.rb.mass; // Spring Constant
        float l = initial_length; // Initial Spring Length
        Vector3 x = point_b - point_a; // Disposition
        Vector3 swing_force = k / l * x; // Modified Hooke's Law application - used for swing
        Vector3 pull_force = -move_direction * grapple_speed; // pull player towards anchor
        // player.rb.AddForce(swing_force + pull_force);
        player.rb.AddForce(swing_force + pull_force);

        if(initial_length > grapple_min_length) { // reduce rope size to min length over time. 
            initial_length -= move_direction.magnitude * grapple_speed * Time.deltaTime;
        }

        if(distance_to_point > grappling_max_distance) {
            StopGrapple();
            return; 
        }
    }

// private void GrappleMovement() {
//     Vector3 point_a = player.transform.position;
//      // This is the anchor point where the grapple is hooked.
    
//     // Current distance between player and grapple point
//     float distance_to_point = Vector3.Distance(point_a, point_b);

//     // Ensure the player stays at the rope's current length by adjusting their position
//     Vector3 move_direction = point_a - point_b; // Direction from the anchor to the player
//     move_direction.Normalize(); // Normalize to get direction only

//     // Calculate the velocity tangent to the swing (perpendicular to the rope's direction)
//     Vector3 player_velocity = player.rb.velocity;
//     Vector3 tangent_velocity = Vector3.Cross(Vector3.Cross(move_direction, player_velocity), move_direction);

//     // Adjust the player's velocity to swing in the tangential direction
//     player.rb.velocity = tangent_velocity + Vector3.down * grappling_gravity_scale; // Apply gravity for a swinging effect

//     // Apply centripetal force to keep player swinging in an arc
//     float swing_force_magnitude = player.rb.mass * Mathf.Pow(tangent_velocity.magnitude, 2) / distance_to_point;
//     Vector3 swing_force = move_direction * swing_force_magnitude;

//     // Apply swing force to the player (pulls them inward toward the rope's fixed length)
//     player.rb.AddForce(swing_force);

//     // Limit the rope length so the player doesn't extend too far
//     if (distance_to_point > initial_length) {
//         // Clamp player's position to the rope's length
//         Vector3 clamped_position = point_b + move_direction * initial_length;
//         player.rb.MovePosition(clamped_position); // Snap player to the max length
//     }

//     // Optional: Reduce rope length over time if you're aiming for a more dynamic feel
//     if (initial_length > grapple_min_length) {
//         initial_length -= grapple_speed * Time.deltaTime;
//     }

//     // If distance exceeds max allowed length, stop the grapple
//     if (distance_to_point > grappling_max_distance) {
//         StopGrapple();
//         return;
//     }
// }



    public void SetGrappleAnchorPointAndStartGrapple(Vector3 position) {
        point_b = position;
        StartGrapple();
    }




     private void RaycastFromCamera()
    {
        // Get the camera's position and direction
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 forwardDirection = Camera.main.transform.forward;

        // Create the ray
        Ray ray = new Ray(cameraPosition, forwardDirection);

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, grappling_max_distance, whatIsGround))
        {
            SetGrappleAnchorPointAndStartGrapple(hit.point);
        }
    }
}
