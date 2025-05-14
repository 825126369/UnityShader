using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Pointer Click Trigger")]
	public class PointerClickTrigger: MonoBehaviour, IPointerClickHandler
	{
        [Serializable]
        public class PointerClickEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private PointerClickEvent m_OnPointerClick = new PointerClickEvent();

        protected PointerClickTrigger()
        {}

        public PointerClickEvent onTrigger
        {
            get { return m_OnPointerClick; }
        }

		void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
