using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Cancel Trigger")]
	public class CancelTrigger: MonoBehaviour, ICancelHandler
	{
        [Serializable]
        public class CancelEvent : UnityEvent {}

        [SerializeField]
        private CancelEvent m_OnCancel = new CancelEvent();

        protected CancelTrigger()
        {}

        public CancelEvent onTrigger
        {
            get { return m_OnCancel; }
        }

		void ICancelHandler.OnCancel(BaseEventData eventData)
		{
			onTrigger.Invoke();
		}
	}
}
