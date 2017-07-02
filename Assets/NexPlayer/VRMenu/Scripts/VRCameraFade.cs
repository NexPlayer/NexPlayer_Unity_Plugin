using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace VRStandardAssets.Utils
{
    // This class is used to fade the entire screen to black (or
    // any chosen colour).  It should be used to smooth out the
    // transition between scenes or restarting of a scene.
    public class VRCameraFade : MonoBehaviour
    {
        public event Action OnFadeComplete;                             // This is called when the fade in or out has finished.


        [SerializeField] private Image m_FadeImage;                     // Reference to the image that covers the screen.
        [SerializeField] private Color m_FadeColor = Color.black;       // The colour the image fades out to.
        [SerializeField] private float m_FadeDuration = 2.0f;           // How long it takes to fade in seconds.
        [SerializeField] private bool m_FadeInOnSceneLoad = false;      // Whether a fade in should happen as soon as the scene is loaded.
        [SerializeField] private bool m_FadeInOnStart = false;          // Whether a fade in should happen just but Updates start.

        
        private bool m_IsFading;                                        // Whether the screen is currently fading.
        private float m_FadeStartTime;                                  // The time when fading started.
        private Color m_FadeOutColor;                                   // This is a transparent version of the fade colour, it will ensure fading looks normal.


        public bool IsFading { get { return m_IsFading; } }


        private void Awake()
        {
			//SceneManager.sceneLoaded += HandleSceneLoaded;

            m_FadeOutColor = new Color(m_FadeColor.r, m_FadeColor.g, m_FadeColor.b, 0f);
            m_FadeImage.enabled = true;
        }


        private void Start()
        {
            // If applicable set the immediate colour to be faded out and then fade in.
            if (m_FadeInOnStart)
            {
                m_FadeImage.color = m_FadeColor;
                FadeIn(true);
            }
        }


		private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            // If applicable set the immediate colour to be faded out and then fade in.
            if (m_FadeInOnSceneLoad)
            {
                m_FadeImage.color = m_FadeColor;
                FadeIn(true);
            }
        }

        
        // Since no duration is specified with this overload use the default duration.
        public void FadeOut(bool fadeAudio)
        {
            FadeOut(m_FadeDuration, fadeAudio);
        }


        public void FadeOut(float duration, bool fadeAudio)
        {
            // If not already fading start a coroutine to fade from the fade out colour to the fade colour.
            if (m_IsFading)
                return;
            StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, duration));
            
        }


        // Since no duration is specified with this overload use the default duration.
        public void FadeIn(bool fadeAudio)
        {
            FadeIn(m_FadeDuration, fadeAudio);
        }


        public void FadeIn(float duration, bool fadeAudio)
        {
            // If not already fading start a coroutine to fade from the fade colour to the fade out colour.
            if (m_IsFading)
                return;
            StartCoroutine(BeginFade(m_FadeColor, m_FadeOutColor, duration));

        }


        public IEnumerator BeginFadeOut (bool fadeAudio)
        {
            yield return StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, m_FadeDuration));
        }


        public IEnumerator BeginFadeOut(float duration, bool fadeAudio)
        {
            yield return StartCoroutine(BeginFade(m_FadeOutColor, m_FadeColor, duration));
        }


        public IEnumerator BeginFadeIn (bool fadeAudio)
        {
            yield return StartCoroutine(BeginFade(m_FadeColor, m_FadeOutColor, m_FadeDuration));
        }


        public IEnumerator BeginFadeIn(float duration, bool fadeAudio)
        {
            yield return StartCoroutine(BeginFade(m_FadeColor, m_FadeOutColor, duration));
        }


        private IEnumerator BeginFade(Color startCol, Color endCol, float duration)
        {
            // Fading is now happening.  This ensures it won't be interupted by non-coroutine calls.
            m_IsFading = true;

            // Execute this loop once per frame until the timer exceeds the duration.
            float timer = 0f;
            while (timer <= duration)
            {
                // Set the colour based on the normalised time.
                m_FadeImage.color = Color.Lerp(startCol, endCol, timer / duration);

                // Increment the timer by the time between frames and return next frame.
                timer += Time.deltaTime;
                yield return null;
            }

            // Fading is finished so allow other fading calls again.
            m_IsFading = false;

            // If anything is subscribed to OnFadeComplete call it.
            if (OnFadeComplete != null)
                OnFadeComplete();
        }

		void OnDestroy()
		{
			//SceneManager.sceneLoaded -= HandleSceneLoaded;
		}
    }
}