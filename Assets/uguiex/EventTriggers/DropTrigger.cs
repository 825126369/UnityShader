using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Drop Trigger")]
	public class DropTrigger: MonoBehaviour, IDropHandler
	{
        [Serializable]
        public class DropEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private DropEvent m_OnDrop = new DropEvent();

        protected DropTrigger()
        {}

        public DropEvent onTrigger
        {
            get { return m_OnDrop; }
        }

		void IDropHandler.OnDrop(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
