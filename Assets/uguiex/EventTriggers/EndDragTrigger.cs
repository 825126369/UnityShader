using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/EndDrag Trigger")]
	public class EndDragTrigger: MonoBehaviour, IEndDragHandler
	{
        [Serializable]
        public class EndDragEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private EndDragEvent m_OnEndDrag = new EndDragEvent();

        protected EndDragTrigger()
        {}

        public EndDragEvent onTrigger
        {
            get { return m_OnEndDrag; }
        }

		void IEndDragHandler.OnEndDrag(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
