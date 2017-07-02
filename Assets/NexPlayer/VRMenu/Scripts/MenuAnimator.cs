using System.Collections;
using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Menu
{
    // This script flips through a series of textures
    // whilst the user is looking at it.
    public class MenuAnimator : MonoBehaviour
    {
        [SerializeField] private MeshRenderer m_ScreenMesh;             // The mesh renderer who's texture will be changed.
        [SerializeField] private VRInteractiveItem m_VRInteractiveItem; // The VRInteractiveItem that needs to be looked at for the textures to play.
        [SerializeField] private Texture[] m_AnimTextures;              // The textures that will be looped through.

        private WaitForSeconds m_FrameRateWait;                         // The delay between frames.
        private int m_CurrentTextureIndex;                              // The index of the textures array.
        private bool m_Playing;                                         // Whether the textures are currently being looped through.

        private void Awake ()
        {
            // The delay between frames is the number of seconds (one) divided by the number of frames that should play during those seconds (frame rate).
            m_FrameRateWait = new WaitForSeconds (2f);
            m_Playing = false;
        }

        private void OnEnable ()
        {
            m_VRInteractiveItem.OnOver += HandleOver;
            m_VRInteractiveItem.OnOut += HandleOut;

            StartCoroutine(PlayTextures());
        }

        private void OnDisable ()
        {
            m_VRInteractiveItem.OnOver -= HandleOver;
            m_VRInteractiveItem.OnOut -= HandleOut;

            StopCoroutine(PlayTextures());
        }

        private void HandleOver ()
        {
            // When the user looks at the VRInteractiveItem the textures should start playing.
            m_Playing = true;
        }

        private void HandleOut ()
        {
            // When the user looks away from the VRInteractiveItem the textures should no longer be playing.
            m_Playing = false;
        }

        private IEnumerator PlayTextures ()
        {
            while (true)
            {
                yield return m_FrameRateWait;
                if (m_Playing)
                {
                    // Set the texture of the mesh renderer to the texture indicated by the index of the textures array.
                    m_ScreenMesh.material.mainTexture = m_AnimTextures[m_CurrentTextureIndex];

                    // Then increment the texture index (looping once it reaches the length of the textures array.
                    m_CurrentTextureIndex = (m_CurrentTextureIndex + 1) % m_AnimTextures.Length;
                }
            }
        }
    }
}