using UnityEngine;
using System.Collections;

public class ChooseMain : MonoBehaviour {

	void Start ()
    {
        ChooseMenu();
    }

    public static void ChooseMenu()
    {
        if (UnityEngine.VR.VRSettings.enabled)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuVR");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

}
