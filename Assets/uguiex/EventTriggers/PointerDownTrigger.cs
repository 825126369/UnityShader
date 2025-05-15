using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Pointer Down Trigger")]
	public class PointerDownTrigger: MonoBehaviour, IPointerDownHandler
	{
        [Serializable]
        public class PointerDownEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private PointerDownEvent m_OnPointerDown = new PointerDownEvent();

        protected PointerDownTrigger()
        {}

        public PointerDownEvent onTrigger
        {
            get { return m_OnPointerDown; }
        }

		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
