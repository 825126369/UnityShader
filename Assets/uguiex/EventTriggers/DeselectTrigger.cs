using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Deselect Trigger")]
	public class DeselectTrigger: MonoBehaviour, IDeselectHandler
	{
        [Serializable]
        public class DeselectEvent : UnityEvent {}

        [SerializeField]
        private DeselectEvent m_OnDeselect = new DeselectEvent();

        protected DeselectTrigger()
        {}

        public DeselectEvent onTrigger
        {
            get { return m_OnDeselect; }
        }

		void IDeselectHandler.OnDeselect(BaseEventData eventData)
		{
			onTrigger.Invoke();
		}
	}
}
