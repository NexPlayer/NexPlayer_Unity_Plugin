using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VRStandardAssets.Utils
{
    // This class works similarly to the SelectionRadial class except
    // it has a physical manifestation in the scene.  This can be
    // either a UI slider or a mesh with the SlidingUV shader.  The
    // functions as a bar that fills up whilst the user looks at it
    // and holds down the Fire1 button.
    public class SelectionSlider : MonoBehaviour
    {
        public event Action OnBarFilled;                                    // This event is triggered when the bar finishes filling.


        [SerializeField] private float m_Duration = 2f;                     // The length of time it takes for the bar to fill.
        [SerializeField] private Slider m_Slider;                           // Optional reference to the UI slider (unnecessary if using a standard Renderer).
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // Reference to the VRInteractiveItem to determine when to fill the bar.
        [SerializeField] private VRInput m_VRInput;                         // Reference to the VRInput to detect button presses.
        [SerializeField] private GameObject m_BarCanvas;                    // Optional reference to the GameObject that holds the slider (only necessary if DisappearOnBarFill is true).
        [SerializeField] private Renderer m_Renderer;                       // Optional reference to a renderer (unnecessary if using a UI slider).
        [SerializeField] private SelectionRadial m_SelectionRadial;         // Optional reference to the SelectionRadial, if non-null the duration of the SelectionRadial will be used instead.
        [SerializeField] private UIFader m_UIFader;                         // Optional reference to a UIFader, used if the SelectionSlider needs to fade out.
        [SerializeField] private Collider m_Collider;                       // Optional reference to the Collider used to detect the user's gaze, turned off when the UIFader is not visible.
        [SerializeField] private bool m_DisableOnBarFill;                   // Whether the bar should stop reacting once it's been filled (for single use bars).
        [SerializeField] private bool m_DisappearOnBarFill;                 // Whether the bar should disappear instantly once it's been filled.


        private bool m_BarFilled;                                           // Whether the bar is currently filled.
        private bool m_GazeOver;                                            // Whether the user is currently looking at the bar.
        private float m_Timer;                                              // Used to determine how much of the bar should be filled.
        private Coroutine m_FillBarRoutine;                                 // Reference to the coroutine that controls the bar filling up, used to stop it if required.


        private const string k_SliderMaterialPropertyName = "_SliderValue"; // The name of the property on the SlidingUV shader that needs to be changed in order for it to fill.


        private void OnEnable ()
        {
            m_VRInput.OnDown += HandleDown;
            m_VRInput.OnUp += HandleUp;

            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
        }


        private void OnDisable ()
        {
            m_VRInput.OnDown -= HandleDown;
            m_VRInput.OnUp -= HandleUp;

            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
        }


        private void Update ()
        {
            if(!m_UIFader)
                return;

            // If this bar is using a UIFader turn off the collider when it's invisible.
            m_Collider.enabled = m_UIFader.Visible;
        }


        public IEnumerator WaitForBarToFill ()
        {
            // If the bar should disappear when it's filled, it needs to be visible now.
            if(m_BarCanvas && m_DisappearOnBarFill)
                m_BarCanvas.SetActive(true);

            // Currently the bar is unfilled.
            m_BarFilled = false;

            // Reset the timer and set the slider value as such.
            m_Timer = 0f;
            SetSliderValue (0f);

            // Keep coming back each frame until the bar is filled.
            while (!m_BarFilled)
            {
                yield return null;
            }

            // If the bar should disappear once it's filled, turn it off.
            if (m_BarCanvas && m_DisappearOnBarFill)
                m_BarCanvas.SetActive(false);
        }


        private IEnumerator FillBar ()
        {
            // When the bar starts to fill, reset the timer.
            m_Timer = 0f;

            // The amount of time it takes to fill is either the duration set in the inspector, or the duration of the radial.
            float fillTime = m_SelectionRadial != null ? m_SelectionRadial.SelectionDuration : m_Duration;

            // Until the timer is greater than the fill time...
            while (m_Timer < fillTime)
            {
                // ... add to the timer the difference between frames.
                m_Timer += Time.deltaTime;

                // Set the value of the slider or the UV based on the normalised time.
                SetSliderValue(m_Timer / fillTime);
                
                // Wait until next frame.
                yield return null;

                // If the user is still looking at the bar, go on to the next iteration of the loop.
                if (m_GazeOver)
                    continue;

                // If the user is no longer looking at the bar, reset the timer and bar and leave the function.
                m_Timer = 0f;
                SetSliderValue (0f);
                yield break;
            }

            // If the loop has finished the bar is now full.
            m_BarFilled = true;

            // If anything has subscribed to OnBarFilled call it now.
            if (OnBarFilled != null)
                OnBarFilled ();

            // If the bar should be disabled once it is filled, do so now.
            if (m_DisableOnBarFill)
                enabled = false;
        }


        private void SetSliderValue (float sliderValue)
        {
            // If there is a slider component set it's value to the given slider value.
            if (m_Slider)
                m_Slider.value = sliderValue;

            // If there is a renderer set the shader's property to the given slider value.
            if(m_Renderer)
                m_Renderer.sharedMaterial.SetFloat (k_SliderMaterialPropertyName, sliderValue);
        }


        private void HandleDown ()
        {
            // If the user is looking at the bar start the FillBar coroutine and store a reference to it.
            if (m_GazeOver)
                m_FillBarRoutine = StartCoroutine(FillBar());
        }


        private void HandleUp ()
        {
            // If the coroutine has been started (and thus we have a reference to it) stop it.
            if(m_FillBarRoutine != null)
                StopCoroutine (m_FillBarRoutine);

            // Reset the timer and bar values.
            m_Timer = 0f;
            SetSliderValue(0f);
        }


        private void HandleOver ()
        {
            // The user is now looking at the bar.
            m_GazeOver = true;
        }


        private void HandleOut ()
        {
            // The user is no longer looking at the bar.
            m_GazeOver = false;

            // If the coroutine has been started (and thus we have a reference to it) stop it.
            if (m_FillBarRoutine != null)
                StopCoroutine(m_FillBarRoutine);

            // Reset the timer and bar values.
            m_Timer = 0f;
            SetSliderValue(0f);
        }
    }
}