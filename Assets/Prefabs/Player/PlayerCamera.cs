using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float mouse_sens_x = 1.0f, mouse_sens_y = 1.0f;
    private float x_rot, y_rot;
    public Transform player_orientation, camera_orientation;
    
    public static PlayerCamera main_instance;    

    private void Start()
    {
        // Lock the cursor to stay ingame
        Cursor.lockState = CursorLockMode.Locked;   
        Cursor.visible = false;

        if(main_instance == null) {
            main_instance = this;
        }
    }

    private void Update()
    {
        // Set the mouseX and Y each frame so the camera rotates accordingly. 
        float mouseX = Input.GetAxisRaw("Mouse X") * mouse_sens_x; 
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouse_sens_y; 

        y_rot += mouseX;

        x_rot -= mouseY;
        x_rot = Mathf.Clamp(x_rot, -90f, 90f);

        transform.rotation = Quaternion.Euler(x_rot, y_rot, 0);
        player_orientation.rotation = Quaternion.Euler(0, y_rot, 0);

    }

    private void LateUpdate()
    {
        transform.position = camera_orientation.position;
    }
}