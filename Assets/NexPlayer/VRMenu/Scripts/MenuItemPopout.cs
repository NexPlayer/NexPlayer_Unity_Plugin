using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Menu
{
    // This class 'pops' each of the menu items out
    // when the user looks at them.
    public class MenuItemPopout : MonoBehaviour
    {
        [SerializeField] private Transform m_Transform;         // Used to control the movement whatever needs to pop out.
        [SerializeField] private VRInteractiveItem m_Item;      // The VRInteractiveItem of whatever should pop out.
        [SerializeField] private float m_PopSpeed = 8f;         // The speed at which the item should pop out.
        [SerializeField] private float m_PopDistance = 0.5f;    // The distance the item should pop out.


        private Vector3 m_StartPosition;                        // The position aimed for when the item should not be popped out.
        private Vector3 m_PoppedPosition;                       // The position aimed for when the item should be popped out.
        private Vector3 m_TargetPosition;                       // The current position being aimed for.


        private void Start ()
        {
            // Store the original position as the one that is not popped out.
            m_StartPosition = m_Transform.position;

            // Calculate the position the item should be when it's popped out.
            m_PoppedPosition = m_Transform.position - m_Transform.forward * m_PopDistance;
        }


        private void Update ()
        {
            // Set the target position based on whether the item is being looked at or not.
            m_TargetPosition = m_Item.IsOver ? m_PoppedPosition : m_StartPosition;

            // Move towards the target position.
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, m_TargetPosition, m_PopSpeed * Time.deltaTime);
        }
    }
}