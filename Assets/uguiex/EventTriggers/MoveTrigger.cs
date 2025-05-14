using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Move Trigger")]
	public class MoveTrigger: MonoBehaviour, IMoveHandler
	{
        [Serializable]
        public class MoveEvent : UnityEvent<AxisEventData> {}

        [SerializeField]
        private MoveEvent m_OnMove = new MoveEvent();

        protected MoveTrigger()
        {}

        public MoveEvent onTrigger
        {
            get { return m_OnMove; }
        }

		void IMoveHandler.OnMove(AxisEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
