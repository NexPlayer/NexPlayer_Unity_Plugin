using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Menu
{
    // In the main menu, a thin bar is shown below each
    // of the scene options.  The bar moves to below
    // whichever option the user is currently looking at.
    // This script moves the bar.
    public class MenuSelectorMover : MonoBehaviour
    {
        [SerializeField] private float m_PopSpeed = 8f;         // How fast the bar pops in and out.
        [SerializeField] private float m_PopDistance = 0.5f;    // How far the bar moves from it's normal position.
        [SerializeField] private float m_MoveSpeed = 7f;        // How fast the bar moves to a new selection when the user changes what they are looking at.
        [SerializeField] private Transform m_ParentTransform;   // The parent object in the selector hierarchy, this has no visual element.
        [SerializeField] private Transform m_ChildTransform;    // The child object in the selector hierarchy, this has a mesh renderer to display the bar.
        [SerializeField] private VRInteractiveItem[] m_Items;   // The VRInteractiveItems that the user can look at to move the selector.


        private Quaternion m_TargetRotation;                    // The rotation that the selector is trying to reach.
        private Vector3 m_StartPosition;                        // The local position of the child transform at the start.
        private Vector3 m_PoppedPosition;                       // The local position of the child transform when it's popped out.
        private Vector3 m_TargetPosition;                       // The local position the child transform is trying to reach.


        void Awake ()
        {
            // Store the start position.
            m_StartPosition = m_ChildTransform.localPosition;

            // Calculate the popped position.
            m_PoppedPosition = m_ChildTransform.localPosition - Vector3.forward * m_PopDistance;
        }

	    
        void Update ()
        {
            // By default the target position of the child transform is unpopped.
            m_TargetPosition = m_StartPosition;

            // Go through each of the interactive items and for the one that the user is looking at, set the target position and rotation.
	        for (int i = 0; i < m_Items.Length; i++)
	        {
	            if (!m_Items[i].IsOver)
                    continue;

	            m_TargetRotation = m_Items[i].transform.rotation;
	            m_TargetPosition = m_PoppedPosition;
	            break;
	        }

            // Set the child's local position to be closer to it's target position based on the speed it should pop in and out.
            m_ChildTransform.localPosition = Vector3.MoveTowards (m_ChildTransform.localPosition, m_TargetPosition,
                m_PopSpeed * Time.deltaTime);

            // Set the parent's rotation to align with whatever VRInteractiveItem is being looked at.
            m_ParentTransform.rotation = Quaternion.Slerp(m_ParentTransform.rotation, m_TargetRotation, m_MoveSpeed * Time.deltaTime);
	    }
    }
}