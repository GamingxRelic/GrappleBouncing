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
    [SerializeField] private float grappling_max_speed = 30f;
    [SerializeField] private float grappling_max_fall_speed = 10f;
    [SerializeField] private float grappling_max_distance = 20f;
    [SerializeField] private float grappling_gravity_scale = 0.4f;
    [SerializeField] private float impulse_strength = 10f;
    [SerializeField] private float impulse_up_strength = 10f;

    [Header("Anchor Variables")]
    [SerializeField] private Transform anchor_launch_point;
    [SerializeField] private GameObject grapple_anchor_prefab;
    [SerializeField] private float anchor_launch_speed = 10f;
    GameObject grapple_anchor_object;


    Vector3 move_direction;

    private LineRenderer line;

    private Vector3 point_a, point_b;

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
            // FireGrappleAnchor();
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

    private void FireGrappleAnchor() {
        if(grapple_anchor_object == null) {
            grapple_anchor_object = Instantiate(grapple_anchor_prefab);
            grapple_anchor_object.transform.position = anchor_launch_point.position;

            GrappleAnchor grapple_anchor = grapple_anchor_object.GetComponent<GrappleAnchor>();
            grapple_anchor.grapple = this;

            Vector3 direction = player.transform.forward + Camera.main.transform.forward;
            Vector3 launch_force = anchor_launch_speed * direction;
            grapple_anchor.Launch(launch_force);
        }
    }
    private void StartGrapple() {
        is_grappling = true;
        line.positionCount = 2;

        // Player Speed Overrides
        player.max_speed_override = grappling_max_speed;
        player.max_fall_speed_override = grappling_max_fall_speed;
        player.gravity_scale_override = grappling_gravity_scale;
    }

    private void StopGrapple() {
        if(grapple_anchor_object != null) {
            Destroy(grapple_anchor_object);
        }
        if(!is_grappling) return;

        is_grappling = false;
        line.positionCount = 0;

        // Add an impulse at the end of the swing
        float distance_to_point = Vector3.Distance(point_a, point_b);
        
        Vector3 impulse = player.transform.up * impulse_up_strength/distance_to_point + player.rb.velocity.normalized * impulse_strength/distance_to_point; 
        player.rb.AddForce(impulse, ForceMode.Impulse);
        Debug.Log(Vector3.Dot(point_a, point_b));

        player.max_speed_override = 0f;
        player.max_fall_speed_override = 0f;
        player.gravity_scale_override = 0f;
    }

    private void GrappleMovement() {
        point_a = player.transform.position;
        move_direction = point_a - point_b;
        move_direction.Normalize();

        float distance_to_point = Vector3.Distance(point_a, point_b);

        player.rb.AddForce(-move_direction * grapple_speed * distance_to_point/10f);

        if(distance_to_point > grappling_max_distance) {
            StopGrapple();
            return; 
        }
    }
    public void SetGrappleAnchorPointAndStartGrapple(Vector3 position) {
        point_b = position;
        StartGrapple();
    }




    [SerializeField] LayerMask whatIsGround;
     private void RaycastFromCamera()
    {
        // Get the camera's position and direction
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 forwardDirection = Camera.main.transform.forward;

        // Create the ray
        Ray ray = new Ray(cameraPosition, forwardDirection);

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, whatIsGround))
        {
            // If we hit something, log the information
            SetGrappleAnchorPointAndStartGrapple(hit.point);
            // You can implement additional logic here, e.g., interact with the hit object
        }
    }
}
