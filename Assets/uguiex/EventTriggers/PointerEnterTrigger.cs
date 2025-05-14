using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Pointer Enter Trigger")]
	public class PointerEnterTrigger: MonoBehaviour, IPointerEnterHandler
	{
        [Serializable]
        public class PointerEnterEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private PointerEnterEvent m_OnPointerEnter = new PointerEnterEvent();

        protected PointerEnterTrigger()
        {}

        public PointerEnterEvent onTrigger
        {
            get { return m_OnPointerEnter; }
        }

		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
