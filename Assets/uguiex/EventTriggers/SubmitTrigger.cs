using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Submit Trigger")]
	public class SubmitTrigger: MonoBehaviour, ISubmitHandler
	{
        [Serializable]
        public class SubmitEvent : UnityEvent {}

        [SerializeField]
        private SubmitEvent m_OnSubmit = new SubmitEvent();

        protected SubmitTrigger()
        {}

        public SubmitEvent onTrigger
        {
            get { return m_OnSubmit; }
        }

		void ISubmitHandler.OnSubmit(BaseEventData eventData)
		{
			onTrigger.Invoke();
		}
	}
}
