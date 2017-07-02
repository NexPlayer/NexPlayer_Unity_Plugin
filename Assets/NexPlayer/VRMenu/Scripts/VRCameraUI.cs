using System;
using UnityEngine;

namespace VRStandardAssets.Utils
{
    // This class ensures that the UI (such as the reticle and selection bar)
    // are set up correctly.
    public class VRCameraUI : MonoBehaviour
    {
        [SerializeField] private Canvas m_Canvas;       // Reference to the canvas containing the UI.


        private void Awake()
        {
            // Make sure the canvas is on.
            m_Canvas.enabled = true;

            // Set its sorting order to the front.
            m_Canvas.sortingOrder = Int16.MaxValue;

            // Force the canvas to redraw so that it is correct before the first render.
            Canvas.ForceUpdateCanvases();
        }
    }
}