using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Components")]
    [SerializeField] MeshFilter mesh;
    public Rigidbody rb;
    [SerializeField] Transform camera_position;
    [SerializeField] GrapplingHook grapple;

    [Header("Movement Variables")]
    [SerializeField] private float walk_speed;
    [SerializeField] private float sprint_speed;
    [SerializeField] private float crouch_speed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 0.9f;
    private float movement_speed;
    private bool sprinting, crouching;
    private Vector3 move_dir;

    [Header("Control Multipliers")]
    [Tooltip("Multiplier for how much control the player has over movement. 1.0 would be perfect control, 0.0 would be no control")]
    [SerializeField, Range(0, 1)] private float ground_control_multiplier = 1.0f;
    [SerializeField, Range(0, 1)] private float air_control_multiplier = 0.25f;
    [SerializeField] private float air_max_speed_multiplier = 2f;
    private float control_multiplier;
    public float max_speed_override;
    private float max_speed;

    [Header("Jumping Variables")]
    [SerializeField] private float jump_force;
    public bool is_grounded;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float gravity_scale = 1f;
    [SerializeField] private float default_gravity_scale = 1f;
    [SerializeField] private float rising_gravity_scale = 1f;
    [SerializeField] private float falling_gravity_scale = 2f;
    [SerializeField] public float gravity_scale_override = 0f;
    [SerializeField] private float falling_offset = 0.25f;
    [SerializeField] private float max_fall_speed = 50.0f;
    public float max_fall_speed_override;



    [Header("Jump Buffering")]
    [SerializeField] private float jump_buffer_time;
    private float jump_timer;
    private bool should_jump;

    void Start()
    {
        PlayerCamera.main_instance.player_orientation = gameObject.transform;
        PlayerCamera.main_instance.camera_orientation = camera_position;   
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
    }

    private void FixedUpdate()
    {
        if(!grapple.is_grappling){
            ApplyMovement();
        }

        HandleGravity();
        CheckSpeedOverrides();
        ClampVelocity();
    }

    private void HandleMovement() {
        // Crouching Input
        if(is_grounded && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))) {
            crouching = true;
            sprinting = false;
        } else {
            crouching = false;
        }


        // Sprinting Input
        if(Input.GetKey(KeyCode.LeftShift) && is_grounded) { // Player can only start sprinting when grounded
            sprinting = true;
            crouching = false;
        } else if(is_grounded){ // Player should only be able to stop sprinting when grounded.
            sprinting = false;
        }

        if(crouching) {
            movement_speed = crouch_speed;
        }
        else if(sprinting) {
            movement_speed = sprint_speed;
        }
        else {
            movement_speed = walk_speed;
        }
        

        // Movement Logic
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        move_dir = transform.forward * verticalInput + transform.right * horizontalInput;
        move_dir.Normalize();
    }

    private void ApplyMovement() {
        if(is_grounded) {
            control_multiplier = ground_control_multiplier;
            if(max_speed_override != 0f) {
                max_speed = movement_speed;
            }
        } else {
            control_multiplier = air_control_multiplier;
            if(max_speed_override != 0f) {
                max_speed = movement_speed * air_max_speed_multiplier;
            }
        }

        if(move_dir != Vector3.zero) { // If there is movement input
            if(is_grounded) { // Move around
                Vector3 target_velocity = move_dir * movement_speed + new Vector3(0f, rb.velocity.y, 0f); 
                rb.velocity = Vector3.Lerp(rb.velocity, target_velocity, acceleration * Time.fixedDeltaTime);
            } else {
                Vector3 target_velocity = move_dir * movement_speed * air_max_speed_multiplier + new Vector3(0f, rb.velocity.y, 0f); 
                rb.velocity = Vector3.Lerp(rb.velocity, target_velocity, acceleration * 2f * air_control_multiplier / Vector3.Distance(rb.velocity, target_velocity) * Time.fixedDeltaTime);
            }
            
        } else { // Otherwise, decelerate
            if(is_grounded)
                rb.velocity = new Vector3(rb.velocity.x * deceleration, rb.velocity.y, rb.velocity.z * deceleration);
            // else
                // rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        }
    }

    private void HandleJump() {
        if(Input.GetButtonDown("Jump")) {
            jump_timer = jump_buffer_time;
        }

        if(jump_timer > 0.0f) {
            jump_timer -= Time.deltaTime;
        }

        if(is_grounded && jump_timer > 0f) {
            should_jump = true;
        }

        if(should_jump) {
            should_jump = false;
            rb.velocity = new Vector3(rb.velocity.x, jump_force, rb.velocity.z);
            jump_timer = 0f;
        }
    }

    private void HandleGravity() {
        if(gravity_scale_override != 0f) {
            gravity_scale = gravity_scale_override;
        }
        else {
            if(!is_grounded) {
                if(rb.velocity.y > falling_offset) { // if rising
                    gravity_scale = rising_gravity_scale;
                }
                else { // if falling
                    gravity_scale = falling_gravity_scale;
                }
            }
            else {
                gravity_scale = default_gravity_scale;
            }
        }
        Vector3 gravity_force = new Vector3(0f, gravity * gravity_scale, 0f);
        rb.AddForce(gravity_force, ForceMode.Acceleration);
    }

    private void CheckSpeedOverrides() {
        if(max_speed_override != 0f) {
            max_speed = max_speed_override;
        }
    }
    private void ClampVelocity() {
        Vector3 clamped_velocity;
        
        clamped_velocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0f, rb.velocity.z), max_speed);

        if(max_fall_speed_override != 0f) {
            clamped_velocity.y = Math.Clamp(rb.velocity.y, -max_fall_speed, max_fall_speed_override);
        } else {
            clamped_velocity.y = Math.Clamp(rb.velocity.y, -max_fall_speed, max_fall_speed);
        }

        rb.velocity = clamped_velocity;
    }
}
