using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Select Trigger")]
	public class SelectTrigger: MonoBehaviour, ISelectHandler
	{
        [Serializable]
        public class SelectEvent : UnityEvent {}

        [SerializeField]
        private SelectEvent m_OnSelect = new SelectEvent();

        protected SelectTrigger()
        {}

        public SelectEvent onTrigger
        {
            get { return m_OnSelect; }
        }

		void ISelectHandler.OnSelect(BaseEventData eventData)
		{
			onTrigger.Invoke();
		}
	}
}
