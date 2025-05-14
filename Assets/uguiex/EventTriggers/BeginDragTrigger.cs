using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Begin Drag Trigger")]
	public class BeginDragTrigger: MonoBehaviour, IBeginDragHandler
	{
        [Serializable]
        public class BeginDragEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private BeginDragEvent m_OnBeginDrag = new BeginDragEvent();

        protected BeginDragTrigger()
        {}

        public BeginDragEvent onTrigger
        {
            get { return m_OnBeginDrag; }
        }

		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
