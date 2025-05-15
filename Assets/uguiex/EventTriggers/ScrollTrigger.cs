using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Scroll Trigger")]
	public class ScrollTrigger: MonoBehaviour, IScrollHandler
	{
        [Serializable]
        public class ScrollEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private ScrollEvent m_OnScroll = new ScrollEvent();

        protected ScrollTrigger()
        {}

        public ScrollEvent onTrigger
        {
            get { return m_OnScroll; }
        }

		void IScrollHandler.OnScroll(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
