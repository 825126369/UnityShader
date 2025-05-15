using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Update Selected Trigger")]
	public class UpdateSelectedTrigger: MonoBehaviour, IUpdateSelectedHandler
	{
        [Serializable]
        public class UpdateSelectedEvent : UnityEvent {}

        [SerializeField]
        private UpdateSelectedEvent m_OnUpdateSelected = new UpdateSelectedEvent();

        protected UpdateSelectedTrigger()
        {}

        public UpdateSelectedEvent onTrigger
        {
            get { return m_OnUpdateSelected; }
        }

		void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
		{
			onTrigger.Invoke();
		}
	}
}
