using System;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.NexCube
{
    public class NexMainCube : MonoBehaviour
    {
        //public Light cubeLight;
        [SerializeField] private VRInteractiveItem m_InteractiveItem;
        [SerializeField] private string m_SceneToLoad;
        [SerializeField] private VRCameraFade m_CameraFade;
        [SerializeField] private SelectionRadial m_SelectionRadial;

        private bool m_GazeOver;

        public UnityStandardAssets.ImageEffects.SunShafts sunScript;

        // Use this for initialization
        void Start()
        {
            this.transform.Rotate(100 * Time.deltaTime, 0, 0);
            if(sunScript!=null)
                sunScript.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            this.transform.Rotate(100 * Time.deltaTime, 0, 0);
        }

        private void OnEnable()
        {
            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
            m_SelectionRadial.OnSelectionComplete += HandleSelectionComplete;
        }

        private void OnDisable()
        {
            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
            m_SelectionRadial.OnSelectionComplete -= HandleSelectionComplete;
        }

        private void HandleOver()
        {
            // When the user looks at the rendering of the scene, show the radial.
            m_SelectionRadial.Show();
            m_GazeOver = true;
            if (sunScript != null)
                sunScript.enabled = true;
        }

        private void HandleOut()
        {
            // When the user looks away from the rendering of the scene, hide the radial.
            m_SelectionRadial.Hide();
            m_GazeOver = false;
            if (sunScript != null)
                sunScript.enabled = false;
        }

        private void HandleSelectionComplete()
        {
            // If the user is looking at the rendering of the scene when the radial's selection finishes, activate the button.
            if (m_GazeOver)
                StartCoroutine(ActivateButton());
        }

        private IEnumerator ActivateButton()
        {
            // If the camera is already fading, ignore.
            if (m_CameraFade.IsFading)
                yield break;

            // Wait for the camera to fade out.
            yield return StartCoroutine(m_CameraFade.BeginFadeOut(true));

            // Load the level.
            SceneManager.LoadScene(m_SceneToLoad, LoadSceneMode.Single);
        }
    }
}