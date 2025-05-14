using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Drag Trigger")]
	public class DragTrigger: MonoBehaviour, IDragHandler
	{
        [Serializable]
        public class DragEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private DragEvent m_OnDrag = new DragEvent();

        protected DragTrigger()
        {}

        public DragEvent onTrigger
        {
            get { return m_OnDrag; }
        }

		void IDragHandler.OnDrag(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
