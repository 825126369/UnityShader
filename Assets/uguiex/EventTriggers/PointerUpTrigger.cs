using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Pointer Up Trigger")]
	public class PointerUpTrigger: MonoBehaviour, IPointerUpHandler
	{
        [Serializable]
        public class PointerUpEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private PointerUpEvent m_OnPointerUp = new PointerUpEvent();

        protected PointerUpTrigger()
        {}

        public PointerUpEvent onTrigger
        {
            get { return m_OnPointerUp; }
        }

		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
