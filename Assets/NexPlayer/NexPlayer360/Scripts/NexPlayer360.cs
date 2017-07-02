using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NexPlayer360 : MonoBehaviour {

    [Tooltip("Main camera of the scene")]
    public Camera cameraToRotate;
    [Tooltip("Mouse speed")]
    public float speedMouseMovement = 1.5f;
    [Tooltip("Cursor mouse that will be used when the left mouse button is clicked")]
    public Texture2D cursorHandGrabbing;
    [Tooltip("Cursor mouse that will be used by default")]
    public Texture2D cursorHandHovering;
    [Tooltip("Touch input speed")]
    public float touchSpeed = 150f;
    [Tooltip("Zoom speed")]
    public float zoomSpeed = 0.2f;
    [Tooltip("Rotation speed")]
    public float rotationSpeed = 1.0f;
    [Tooltip("Minimum field of view")]
    public float minFieldOfView = 40;
    [Tooltip("Maximum field of view")]
    public float maxFieldOfView = 70;

    public NexUIController toogleUI;

    // Private variables
    private AutomaticGroundLeveler agl;

    private bool rotating = false;
    private bool zooming = false;
    private Vector2 startVector;

    private Vector2 lastTouchPostion;
    private Vector2 lastTouchPostionZoom0;
    private Vector2 lastTouchPostionZoom1;

    private int thresholdMovement = 10;

    private Quaternion latestAttitude = Quaternion.identity;

    private Gyroscope gyro;
    private bool firstProcessOfAttitude = true;

    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 middleOfTheCursorGrabbing;
    private Vector2 middleOfTheCursorHovering;

    void Awake()
    {
        agl = new AutomaticGroundLeveler();

        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        }

        middleOfTheCursorGrabbing = new Vector2(cursorHandGrabbing.width / 2, cursorHandGrabbing.height / 2);
        middleOfTheCursorHovering = new Vector2(cursorHandHovering.width / 2, cursorHandHovering.height / 2);

        #if !UNITY_EDITOR && UNITY_WEBGL
        // Otherwise it won't work in WebGL
             cursorMode = CursorMode.ForceSoftware;
        #endif

        Cursor.SetCursor(cursorHandHovering, middleOfTheCursorHovering, cursorMode);
    }

    void Update ()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Cursor.SetCursor(cursorHandGrabbing, middleOfTheCursorGrabbing, cursorMode);
        }

        if (Input.GetButton("Fire1") && Input.touchCount == 0 && toogleUI.IsPointerOverGameObject())
        {
            // Handle the mouse movement
            cameraToRotate.transform.Rotate(Vector3.down * Input.GetAxis("Mouse X") * speedMouseMovement);
            cameraToRotate.transform.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * speedMouseMovement);
        }
        else ProcessTouchInput();

        if (SystemInfo.supportsGyroscope) ProcessAttitude();

        agl.AutomaticGroundLevelerStep(cameraToRotate.transform, latestAttitude, rotating);

        if (Input.GetButtonUp("Fire1"))
        {
            Cursor.SetCursor(cursorHandHovering, middleOfTheCursorHovering, cursorMode);
        }
    }

    void OnDisable()
    {
        // Sets the default cursor
        Cursor.SetCursor(null, middleOfTheCursorHovering, cursorMode);
    }

    private void ProcessAttitude()
    {
        // Handle the attitude from the gyroscope
        Quaternion currentRotation = gyro.attitude;

        // Do not change this unless you understand exactly what it does
        // Unity attitude's reference system does not match the one present in the Unity Camera
        // Remaps the coordinate system to be compatible with the Unity Camera system
        currentRotation = new Quaternion(currentRotation.x, currentRotation.y, -currentRotation.z, -currentRotation.w);
        // Performs a rotation around the global frame (or world transform) to make it point properly
        currentRotation =  Quaternion.Euler(90, 0, -90) *currentRotation;
        if (!firstProcessOfAttitude)
            cameraToRotate.transform.rotation *= (Quaternion.Inverse(latestAttitude) * currentRotation);
        else firstProcessOfAttitude = false;

        latestAttitude = currentRotation;
    }

    private void ProcessTouchInput()
    {
        // Handle the touch movement, zoom and rotation
        if (Input.touchCount > 0 && toogleUI.IsPointerOverGameObject())
        {
            if (Input.touchCount == 2)
            {
                processZoom();
                processRotation();
            }

            if (Input.touchCount == 1)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    // Get movement of the finger since last frame
                    Vector2 touchDeltaPosition = Input.GetTouch(0).position - lastTouchPostion;

                    // Move object across XY plane
                    float dX = (touchDeltaPosition.y / touchSpeed);
                    float dY = -1 * (touchDeltaPosition.x / touchSpeed);
                    if (Mathf.Abs(dX) < thresholdMovement && Mathf.Abs(dY) < thresholdMovement * 2)
                    {
                        cameraToRotate.transform.Rotate(Vector3.up * dY);
                        cameraToRotate.transform.Rotate(Vector3.right * dX);
                    }
                }

                lastTouchPostion = Input.GetTouch(0).position;
            }
        }
        if (Input.touchCount < 1)
        {
            rotating = false;
            zooming = false;
        }
    }

    private void processZoom()
    {
        // Store both touches.
        Touch touchZero = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        if (zooming)
        {
            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - (touchZero.position - lastTouchPostionZoom0);
            Vector2 touchOnePrevPos = touchOne.position - (touchOne.position - lastTouchPostionZoom1);

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            cameraToRotate.fieldOfView += deltaMagnitudeDiff * zoomSpeed;

            if (cameraToRotate.fieldOfView < minFieldOfView) cameraToRotate.fieldOfView = minFieldOfView;
            else if (cameraToRotate.fieldOfView > maxFieldOfView) cameraToRotate.fieldOfView = maxFieldOfView;
        }

        zooming = true;

        lastTouchPostionZoom0 = touchZero.position;
        lastTouchPostionZoom1 = touchOne.position;
    }

    private void processRotation()
    {
        if (!rotating)
        {
            startVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
            rotating = startVector.sqrMagnitude > 0;
        }
        else
        {
            var currVector = Input.GetTouch(1).position - Input.GetTouch(0).position;
            var angleOffset = Vector2.Angle(startVector, currVector);
            var LR = Vector3.Cross(startVector, currVector);

            if (angleOffset > 0)
            {
                if (LR.z > 0)
                {
                    // Anticlockwise
                    angleOffset = -1 * angleOffset;
                }
            }

            angleOffset *= rotationSpeed;
            cameraToRotate.transform.Rotate(Vector3.forward * angleOffset);

            startVector = currVector;
        }
    }
}
