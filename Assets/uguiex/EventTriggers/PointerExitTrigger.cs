using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Pointer Exit Trigger")]
	public class PointerExitTrigger: MonoBehaviour, IPointerExitHandler
	{
        [Serializable]
        public class PointerExitEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private PointerExitEvent m_OnPointerExit = new PointerExitEvent();

        protected PointerExitTrigger()
        {}

        public PointerExitEvent onTrigger
        {
            get { return m_OnPointerExit; }
        }

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
