using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public Text titleScene;
    public InputField inputNormalURL;
    public Vector3 Pivot;
    public Button buttonHLSScene;
    public Button buttonHLSURL;
    public Button button360Scene;
    public Button button360URL;
    public Button buttonQuit;

    private bool launchHLSScene = false;

    public void Awake()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        Vector3 pos = buttonHLSScene.transform.position;
        pos.y = 2f;
        buttonHLSScene.transform.position = pos;

        pos = buttonHLSURL.transform.position;
        pos.y = 2f;
        buttonHLSURL.transform.position = pos;

        pos = button360Scene.transform.position;
        pos.y = -8f;
        button360Scene.transform.position = pos;

        pos = button360URL.transform.position;
        pos.y = -8f;
        button360URL.transform.position = pos;

        buttonQuit.gameObject.SetActive(false);
#endif
        NexPlayer.sharedURL = null;
    }

    public void ToogleHLSScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("NexPlayer raw video");
    }

    public void Toogle360Scene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("NexPlayer360");
    }

    public void ToogleURLHLSScene()
    {
        launchHLSScene = true;
        titleScene.text = "Normal Scene";
    }

    public void ToogleURL360Scene()
    {
        launchHLSScene = false;
        titleScene.text = "360 Scene";
    }

    public void ToogleGO()
    {
        NexPlayer.sharedURL = inputNormalURL.text.ToString();

        if (launchHLSScene)
            ToogleHLSScene();
        else
        {
            StereoMode.stereoModeTypeFromOtherScene = StereoMode.StereoModeEnum.Mono;
            Toogle360Scene();
        }
    }

    public void ToogleClose()
    {
        NexPlayer.sharedURL = null;
    }

    public void ToogleQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_EDITOR && !UNITY_WEBGL
        Application.Quit();
#endif
    }
}
