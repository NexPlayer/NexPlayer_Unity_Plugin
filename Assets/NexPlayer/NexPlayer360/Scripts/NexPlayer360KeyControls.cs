using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NexPlayer360KeyControls : MonoBehaviour {
    
    [Tooltip("Main camera of the scene")]
    public Camera cameraToRotate;

    [Tooltip("Key used to represent a movement to the right")]
    public KeyCode right = KeyCode.RightArrow;
    [Tooltip("Key used to represent a movement to the right")]
    public KeyCode left = KeyCode.LeftArrow;
    [Tooltip("Key used to represent a movement to the top")]
    public KeyCode up = KeyCode.UpArrow;
    [Tooltip("Key used to represent a movement to the bottom")]
    public KeyCode down = KeyCode.DownArrow;

    [Tooltip("Key speed")]
    public float speedKeyMovement = 80.0f;

	void Update () {
        if (Input.GetKey(right))
            cameraToRotate.transform.Rotate(Vector3.down * -1 * speedKeyMovement * Time.deltaTime);

        if (Input.GetKey(left))
            cameraToRotate.transform.Rotate(Vector3.down * speedKeyMovement * Time.deltaTime);

        if (Input.GetKey(up))
            cameraToRotate.transform.Rotate(Vector3.right * -1 * speedKeyMovement * Time.deltaTime);

        if (Input.GetKey(down))
            cameraToRotate.transform.Rotate(Vector3.right * speedKeyMovement * Time.deltaTime);
    }
}
