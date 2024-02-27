using UnityEngine;
using UnityEngine.EventSystems;

namespace AdvancedCompany
{
    public class DressUpDrag : MonoBehaviour, IDragHandler
    {
        public bool Dragging = false;
        public Vector2 Delta = Vector2.zero;
        private Vector2 LastDrag = Vector2.zero;
        private bool DraggedLastFrame = false;

        public void OnDrag(PointerEventData eventData)
        {
            if (Dragging)
            {
                Delta = LastDrag - eventData.position;
            }
            Dragging = true;
            DraggedLastFrame = true;
            LastDrag = eventData.position;
            eventData.Use();
        }

        void LateUpdate()
        {
            Delta = Vector2.zero;
            if (!DraggedLastFrame)
            {
                Dragging = false;
                LastDrag = Vector2.zero;
            }
            DraggedLastFrame = false;
        }

    }
}
