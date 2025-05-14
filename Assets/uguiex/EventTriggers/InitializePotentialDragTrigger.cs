using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityEngine.EventSystems
{
    [AddComponentMenu("Event Triggers/Initialize Potential Drag Trigger")]
	public class InitializePotentialDragTrigger: MonoBehaviour, IInitializePotentialDragHandler
	{
        [Serializable]
        public class InitializePotentialDragEvent : UnityEvent<PointerEventData> {}

        [SerializeField]
        private InitializePotentialDragEvent m_OnInitializePotentialDrag = new InitializePotentialDragEvent();

        protected InitializePotentialDragTrigger()
        {}

        public InitializePotentialDragEvent onTrigger
        {
            get { return m_OnInitializePotentialDrag; }
        }

		void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
		{
			onTrigger.Invoke(eventData);
		}
	}
}
