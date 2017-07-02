using UnityEngine;
using System.Collections;

public class NexMainCube : MonoBehaviour {

    public Light cubeLight;

	// Update is called once per frame
	void Update () {
        this.transform.Rotate(100*Time.deltaTime, 0, 0);
    }

    void OnMouseUp()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("NexPlayer game objects");
    }

    void OnMouseEnter()
    {
        cubeLight.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        cubeLight.gameObject.SetActive(false);
    }
}