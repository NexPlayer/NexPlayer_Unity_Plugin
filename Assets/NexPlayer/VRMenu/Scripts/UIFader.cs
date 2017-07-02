using System;
using UnityEngine;
using System.Collections;

namespace VRStandardAssets.Utils
{
    // This class is used to fade in and out groups of UI
    // elements.  It contains a variety of functions for
    // fading in different ways.
    public class UIFader : MonoBehaviour
    {
        public event Action OnFadeInComplete;                   // This event is triggered when the UI elements have finished fading in.
        public event Action OnFadeOutComplete;                  // This event is triggered when the UI elements have finished fading out.


        [SerializeField] private float m_FadeSpeed = 1f;        // The amount the alpha of the UI elements changes per second.
        [SerializeField] private CanvasGroup[] m_GroupsToFade;  // All the groups of UI elements that will fade in and out.


        private bool m_Fading;                                  // Whether the UI elements are currently fading in or out.


        public bool Visible { get; private set; }               // Whether the UI elements are currently visible.


        public IEnumerator WaitForFadeIn()
        {
            // Keep coming back each frame whilst the groups are currently fading.
            while (m_Fading)
            {
                yield return null;
            }

            // Return once the FadeIn coroutine has finished.
            yield return StartCoroutine (FadeIn ());
        }


        public IEnumerator InteruptAndFadeIn ()
        {
            // Stop all fading that is currently happening.
            StopAllCoroutines ();

            // Return once the FadeIn coroutine has finished.
            yield return StartCoroutine(FadeIn());
        }


        public IEnumerator CheckAndFadeIn ()
        {
            // If not already fading return once the FadeIn coroutine has finished.
            if (!m_Fading)
                yield return StartCoroutine (FadeIn ());
        }


        public IEnumerator FadeIn()
        {
            // Fading has now started.
            m_Fading = true;

            // Fading needs to continue until all the groups have finishing fading in so we need to base that on the lowest alpha.
            float lowestAlpha;

            do
            {
                // Assume the lowest alpha has faded in already.
                lowestAlpha = 1f;

                // Go through all the groups...
                for (int i = 0; i < m_GroupsToFade.Length; i++)
                {
                    // ... and increment their alpha based on the fade speed.
                    m_GroupsToFade[i].alpha += m_FadeSpeed * Time.deltaTime;

                    // Also we need to check what the lowest alpha is.
                    if (m_GroupsToFade[i].alpha < lowestAlpha)
                        lowestAlpha = m_GroupsToFade[i].alpha;
                }

                // Wait until next frame.
                yield return null;
            }
            // Continue doing this until the lowest alpha is one or greater.
            while (lowestAlpha < 1f);

            // If there is anything subscribed to OnFadeInComplete, call it.
            if (OnFadeInComplete != null)
                OnFadeInComplete();

            // Fading has now finished.
            m_Fading = false;

            // Since everthing has faded in now, it is visible.
            Visible = true;
        }


        // The following functions are identical to the previous ones but fade the CanvasGroups out instead.
        public IEnumerator WaitForFadeOut ()
        {
            while (m_Fading)
            {
                yield return null;
            }

            yield return StartCoroutine (FadeOut ());
        }


        public IEnumerator InteruptAndFadeOut ()
        {
            StopAllCoroutines ();
            yield return StartCoroutine (FadeOut ());
        }


        public IEnumerator CheckAndFadeOut()
        {
            if (!m_Fading)
                yield return StartCoroutine(FadeOut());
        }


        public IEnumerator FadeOut ()
        {
            m_Fading = true;

            float highestAlpha;

            do
            {
                highestAlpha = 0f;

                for (int i = 0; i < m_GroupsToFade.Length; i++)
                {
                    m_GroupsToFade[i].alpha -= m_FadeSpeed * Time.deltaTime;

                    if (m_GroupsToFade[i].alpha > highestAlpha)
                        highestAlpha = m_GroupsToFade[i].alpha;
                }

                yield return null;
            }
            while (highestAlpha > 0f);

            if (OnFadeOutComplete != null)
                OnFadeOutComplete();

            m_Fading = false;

            Visible = false;
        }


        // These functions are used if fades are required to be instant.
        public void SetVisible ()
        {
            for (int i = 0; i < m_GroupsToFade.Length; i++)
            {
                m_GroupsToFade[i].alpha = 1f;
            }

            Visible = true;
        }


        public void SetInvisible ()
        {
            for (int i = 0; i < m_GroupsToFade.Length; i++)
            {
                m_GroupsToFade[i].alpha = 0f;
            }

            Visible = false;
        }
    }
}
