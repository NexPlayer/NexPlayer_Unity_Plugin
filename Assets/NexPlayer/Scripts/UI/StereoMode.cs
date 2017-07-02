using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StereoMode : MonoBehaviour {
    public enum StereoModeEnum { None, Mono, StereoOverUnder, StereoSideBySide };

    public StereoModeEnum stereoModeType;

    public Camera rightCamera;
    public Camera leftCamera;

    public Material replacementMainMaterial;
    public Material replacementLeftMaterial;
    public Material replacementRightMaterial;
    public Material replacementOverMaterial;
    public Material replacementUnderMaterial;

    public static StereoModeEnum stereoModeTypeFromOtherScene = StereoModeEnum.None;

    private Renderer mainRenderer;
    private RawImage mainRawImage;

    // callback to be called before any camera starts rendering
    public void MyPreRender(Camera cam)
    {
        if (mainRenderer != null)
        {
            if (stereoModeType == StereoModeEnum.Mono)
            {
                ChangeMaterial(replacementMainMaterial);
            }
            else
            {
                if (cam == rightCamera)
                {
                    if (stereoModeType == StereoModeEnum.StereoOverUnder)
                    {
                        ChangeMaterial(replacementOverMaterial);
                    }
                    else
                    {
                        ChangeMaterial(replacementRightMaterial);
                    }
                }
                else if (cam == leftCamera)
                {
                    if (stereoModeType == StereoModeEnum.StereoOverUnder)
                    {
                        ChangeMaterial(replacementUnderMaterial);
                    }
                    else
                    {
                        ChangeMaterial(replacementLeftMaterial);
                    }
                }
                else
                {
                    // main camera
                    if (stereoModeType == StereoModeEnum.StereoOverUnder)
                    {
                        ChangeMaterial(replacementOverMaterial);
                    }
                    else
                    {
                        ChangeMaterial(replacementRightMaterial);
                    }
                }
            }
        }
        else if (mainRawImage != null)
        {
            if (stereoModeType == StereoModeEnum.Mono)
            {
                mainRawImage.uvRect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
            }
            else
            {
                if (cam == rightCamera)
                {
                    if (stereoModeType == StereoModeEnum.StereoOverUnder)
                    {
                        mainRawImage.uvRect = new Rect(0.0f, 0.5f, 1.0f, 0.5f);
                    }
                    else
                    {
                        mainRawImage.uvRect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
                    }
                }
                else if (cam == leftCamera)
                {
                    if (stereoModeType == StereoModeEnum.StereoOverUnder)
                    {
                        mainRawImage.uvRect = new Rect(0.0f, 0.0f, 1.0f, 0.5f);
                    }
                    else
                    {
                        mainRawImage.uvRect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
                    }
                }
                else
                {
                    // main camera
                    if (stereoModeType == StereoModeEnum.StereoOverUnder)
                    {
                        mainRawImage.uvRect = new Rect(0.0f, 0.5f, 1.0f, 0.5f);
                    }
                    else
                    {
                        mainRawImage.uvRect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
                    }
                }
            }
        }
    }

    private void ChangeMaterial(Material newMaterial)
    {
        Texture tex = mainRenderer.material.mainTexture;
        mainRenderer.material = newMaterial;
        mainRenderer.material.mainTexture = tex;
    }

    public void OnEnable()
    {
        if (stereoModeTypeFromOtherScene != StereoModeEnum.None)
            stereoModeType = stereoModeTypeFromOtherScene;

        stereoModeTypeFromOtherScene = StereoModeEnum.None;

        if (stereoModeType == StereoModeEnum.None)
            stereoModeType = StereoModeEnum.Mono;

        mainRenderer = GetComponent<Renderer>();
        mainRawImage = GetComponent<RawImage>();

        // register the callback when enabling object
        Camera.onPreRender += MyPreRender;
    }
    public void OnDisable()
    {
        // remove the callback when disabling object
        Camera.onPreRender -= MyPreRender;
    }
}