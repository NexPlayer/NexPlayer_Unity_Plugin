using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NexUIController : MonoBehaviour {
    [System.Serializable]
    public struct UIComponentForVR
    {
        [Tooltip("GameObject that holds the UI canvas")]
        public GameObject gameObjectThatHoldUICanvas;
        [Tooltip("Canvas which render mode will be modified in VR")]
        public Canvas canvasToModifyInVR;
        [Tooltip("Sorting order of the canvas")]
        public int sortingOrderCanvas;
        [Tooltip("Distance where the canvas will be placed")]
        public float distanceTheCanvasWillBePlaced;
        [Tooltip("Should the object be centered with the position where the user is looking?")]
        public bool centerWithUser;
    }

    [Tooltip("GameObject to be toggled")]
    public GameObject gameObjectToToggle;
    [Tooltip("Each UI component to be toogled in VR")]
    public UIComponentForVR[] UIComponentsToBeToggledInIVR;
    [Tooltip("Cardboard button that will be hidden if it's not supported")]
    public GameObject cardboardButton;
    [Tooltip("GameObjects to be shown exclusively in VR")]
    public GameObject[] VRExclusiveObjects;
    [Tooltip("Main camera")]
    public Transform mainCamera;
    [Tooltip("Layers containing non UI elements")]
    public LayerMask exclusionLayers;
    [Tooltip("SelectionRadial to be toggled")]
    public VRStandardAssets.Utils.SelectionRadial radial;
    [Tooltip("Reticle to be toggled")]
    public VRStandardAssets.Utils.Reticle reticle;
    
    private Vector3 lastMousePosition;
    private bool isCoroutineRunning = false;

    void Start()
    {
        if (cardboardButton != null)
            cardboardButton.SetActive(DoesTheBuildSupportCardboard());

        if (radial != null)
            radial.Hide();
        if (reticle != null)
            reticle.Hide();

        gameObjectToToggle.SetActive(false);
        ToogleUI();
    }

    void Update ()
    {
        if (IsPointerOverGameObject() && HasThePointerBeingClicked())
        {
            ToogleUI();
        }
    }
    
    /// <summary>
    /// Toggles the UI visibility taking into account the VR mode
    /// </summary>
    private void ToogleUI()
    {
        gameObjectToToggle.SetActive(!gameObjectToToggle.activeSelf);

        foreach(GameObject tempObject in VRExclusiveObjects)
        {
            tempObject.SetActive(UnityEngine.VR.VRSettings.enabled ? true : false);
        }

        if (UnityEngine.VR.VRSettings.enabled)
        {
            if (UIComponentsToBeToggledInIVR != null)
            {
                foreach (UIComponentForVR UIComponent in UIComponentsToBeToggledInIVR)
                {
                    UIComponent.gameObjectThatHoldUICanvas.transform.position = new Vector3(0, 0, 0);

                    // For testing purposes in the editor. If this is used, comment the InputTracking.GetLocalRotation line
                    // gameObjectToToggle.transform.rotation = Camera.main.transform.rotation;
                    if (UIComponent.centerWithUser)
                        UIComponent.gameObjectThatHoldUICanvas.transform.rotation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.CenterEye);
                    else
                        UIComponent.gameObjectThatHoldUICanvas.transform.rotation = Quaternion.identity;

                    // Change the canvas mode
                    UIComponent.canvasToModifyInVR.renderMode = RenderMode.WorldSpace;
                    RectTransform transformCanvas = UIComponent.canvasToModifyInVR.GetComponent<RectTransform>();
                    // Set the scale
                    transformCanvas.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                    // Set the position
                    transformCanvas.localPosition = new Vector3(0, 0, 0);
                    // The width and height will be the same as the screen of the phone (to keep the perspective)

                    UIComponent.gameObjectThatHoldUICanvas.transform.position += UIComponent.gameObjectThatHoldUICanvas.transform.forward * UIComponent.distanceTheCanvasWillBePlaced;
                }
            }
        }
        else
        {
            if (UIComponentsToBeToggledInIVR != null)
            {
                foreach (UIComponentForVR UIComponent in UIComponentsToBeToggledInIVR)
                {
                    // Reset default values
                    UIComponent.gameObjectThatHoldUICanvas.transform.position = new Vector3(0, 0, 0);
                    UIComponent.gameObjectThatHoldUICanvas.transform.rotation = Quaternion.identity;
      
                    RectTransform transformCanvas = UIComponent.canvasToModifyInVR.GetComponent<RectTransform>();
                    transformCanvas.localScale = new Vector3(1, 1, 1);
                    transformCanvas.localPosition = new Vector3(0, 0, 0);

                    UIComponent.canvasToModifyInVR.renderMode = RenderMode.ScreenSpaceOverlay;
                    UIComponent.canvasToModifyInVR.sortingOrder = UIComponent.sortingOrderCanvas;
                }
            }
        }

        OnUIChange();
    }

    public void OnUIChange()
    {
        if (radial != null && reticle != null)
        {
            if (!IsUIVisible() || !UnityEngine.VR.VRSettings.enabled)
            {
                radial.Hide();
                reticle.Hide();
            }
            else
            {
                if (UnityEngine.VR.VRSettings.enabled)
                {
                    radial.Hide();
                    reticle.Show();
                }
            }
        }
    }

    private bool IsUIVisible()
    {
        return gameObjectToToggle.activeSelf;
    }

    public void ToogleVR()
    {
        if (DoesTheBuildSupportCardboard() && !isCoroutineRunning)
            StartCoroutine(ToogleVRMode());
    }

    public void RecenterVR()
    {
        if (gameObjectToToggle.activeSelf) ToogleUI();

        UnityEngine.VR.InputTracking.Recenter();
    }

    /// <summary>
    /// Informs if cardboard is present in the build
    /// </summary>
    private bool DoesTheBuildSupportCardboard()
    {
        #if UNITY_5_6_OR_NEWER
            return System.Array.Exists(UnityEngine.VR.VRSettings.supportedDevices, s => s.ToLower().Contains("cardboard"));
        #else
            return false;
        #endif
    }

    /// <summary>
    /// Coroutine that toggles VR
    /// </summary>
    private IEnumerator ToogleVRMode()
    {
        isCoroutineRunning = true;

        if (!UnityEngine.VR.VRSettings.enabled)
        {
            #if UNITY_5_6_OR_NEWER
                UnityEngine.VR.VRSettings.LoadDeviceByName("cardboard");
            #endif
        }

        // Wait until the VR device has loaded
        yield return null;

        // Recenter the camera
        Camera.main.transform.rotation = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);
        UnityEngine.VR.VRSettings.enabled = !UnityEngine.VR.VRSettings.enabled;

        // This is a workaround for Unity bad behaviour(it doesn't maintain the original aspect ratio)
        Camera.main.ResetAspect();

        gameObjectToToggle.SetActive(false);
        ToogleUI();

        // Recenter the VR input tracking
        UnityEngine.VR.InputTracking.Recenter();

        isCoroutineRunning = false;
    }
    
    /// <summary>
    /// Informs whether the mouse has been clicked or not
    /// </summary>
    private bool HasThePointerBeingClicked()
    {
        bool hasThePointerBeingClicked = false;
        if (Input.GetButtonDown("Fire1"))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            hasThePointerBeingClicked = Vector3.Distance(lastMousePosition, Input.mousePosition) < 10;
        }

        return hasThePointerBeingClicked;
    }

    /// <summary>
    /// Provides information to know if the pointer or touch input is over the game objects without being blocked by any UI element
    /// </summary>
    /// <returns>true if no UI element is blocking the pointer our touch</returns>
    public bool IsPointerOverGameObject()
    {
        bool isOverGameObject = true;
        if (!UnityEngine.VR.VRSettings.enabled)
        {
            if (Input.touchCount > 0)
            {
                Touch[] touches = Input.touches;
                int i = 0;
                while (isOverGameObject && i < touches.Length)
                {
                    if (EventSystem.current.IsPointerOverGameObject(touches[i].fingerId))
                    {
                        // you touched at least one UI element
                        isOverGameObject = false;
                    }

                    i++;
                }
            }
            else
            {
                isOverGameObject = !EventSystem.current.IsPointerOverGameObject();
            }
        }
        else
        {
            // In VR mode a raycast is used to determine if the reticle is over the UI

            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(mainCamera.position, mainCamera.forward);
            RaycastHit hit;
            // Do the raycast forwards to see if we hit an interactive item
            isOverGameObject = !Physics.Raycast(ray, out hit, 500f, ~exclusionLayers);
        }

        return isOverGameObject;
    }
}
