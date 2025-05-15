using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using System;
#endif

using LuaInterface;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/VirtualJoystick"), RequireComponent(typeof(RectTransform))]
    public class VirtualJoystick : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {

        bool _forceShow = false;

        public bool ForceShow
        {
            get { return _forceShow; }
            set
            {
                _forceShow = value;
            }
        }


        [SerializeField]
        bool _isRelocation;
        /// <summary>
        /// 点击时是否设置摇杆位置到点击位置
        /// </summary>
        public bool IsRelocation
        {
            get { return _isRelocation; }
            set
            {
                _isRelocation = value;
            }
        }

        [SerializeField]
        bool _isAutoHide;
        /// <summary>
        /// 不操作时是否隐藏
        /// </summary>
        public bool IsAutoHide
        {
            get { return _isAutoHide; }
            set
            {
                _isAutoHide = value;
            }
        }

        [SerializeField]
        bool _isAutoOrigin;
        /// <summary>
        /// 不操作时是否返回原点
        /// </summary>
        public bool IsAutoOrigin
        {
            get { return _isAutoOrigin; }
            set
            {
                _isAutoOrigin = value;
            }
        }
        public float OriginX = 0;
        public float OriginY = 0;

        [SerializeField]
        RectTransform _joystickGroup = null;
        public RectTransform JoystickGroup
        {
            get { return _joystickGroup; }
            set
            {
                _joystickGroup = value;
            }
        }

        [SerializeField]
        RectTransform _joystickBackGround = null;
        public RectTransform JoystickBackGround
        {
            get { return _joystickBackGround; }
            set
            {
                _joystickBackGround = value;
            }
        }


        [SerializeField]
        Transform _joystickDir = null;
        public Transform JoystickDir
        {
            get { return _joystickDir; }
            set
            {
                _joystickDir = value;
            }
        }


        [SerializeField, Tooltip("The child graphic that will be moved around")]
        RectTransform _joystickGraphic;

        

        GameObject lineObj1 = null;
        RectTransform lineObjRT1 = null;

        GameObject lineObj2 = null;
        RectTransform lineObjRT2 = null;

        GameObject lineObj3 = null;
        RectTransform lineObjRT3 = null;

        GameObject lineObj4 = null;
        RectTransform lineObjRT4 = null;

        GameObject lineObj5 = null;
        RectTransform lineObjRT5 = null;

        GameObject lineObj6 = null;
        RectTransform lineObjRT6 = null;

        GameObject lineObj7 = null;
        RectTransform lineObjRT7 = null;

        GameObject lineObj8 = null;
        RectTransform lineObjRT8 = null;

        protected override void OnDestroy()
        {
            GameObject.Destroy(lineObj1);
            GameObject.Destroy(lineObj2);
            GameObject.Destroy(lineObj3);
            GameObject.Destroy(lineObj4);
            GameObject.Destroy(lineObj5);
            GameObject.Destroy(lineObj6);
            GameObject.Destroy(lineObj7);
            GameObject.Destroy(lineObj8);

            lineObj1 = null;
            lineObjRT1 = null;
            
            lineObj2 = null;
            lineObjRT2 = null;
            
            lineObj3 = null;
            lineObjRT3 = null;
            
            lineObj4 = null;
            lineObjRT4 = null;
            
            lineObj5 = null;
            lineObjRT5 = null;
            
            lineObj6 = null;
            lineObjRT6 = null;
            
            lineObj7 = null;
            lineObjRT7 = null;
            
            lineObj8 = null;
            lineObjRT8 = null;
        }

        [SerializeField]
        public Sprite LineTexture = null;

        [SerializeField]
        public float DeltaR = 0.0f;
        [SerializeField]
        public float lineWidth = 4.0f;
        [SerializeField]
        public Color lineColor = Color.red;


        public RectTransform JoystickGraphic
        {
            get { return _joystickGraphic; }
            set
            {
                _joystickGraphic = value;
                UpdateJoystickGraphic();
            }
        }

        [SerializeField]
        RectTransform _joystickBackUI = null;
        public RectTransform JoystickBack
        {
            get { return _joystickBackUI; }
            set
            {
                _joystickBackUI = value;
            }
        }

        [SerializeField]        //to display in inspector
        Vector2 _axis;

        [SerializeField, Tooltip("How fast the joystick will go back to the center")]
        float _spring = 25;
        public float Spring
        {
            get { return _spring; }
            set { _spring = value; }
        }

        [SerializeField, Tooltip("How close to the center that the axis will be output as 0")]
        float _deadZone = .1f;
        public float DeadZone
        {
            get { return _deadZone; }
            set { _deadZone = value; }
        }

        [Tooltip("Customize the output that is sent in OnValueChange")]
        public AnimationCurve outputCurve = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));

        public JoystickMoveEvent onValueChange;

        public JoystickPressEvent onPress;
        public bool IsPressed
        {
            get { return _IsControlled; }
        }

        public Vector2 JoystickAxis
        {
            get
            {
                Vector2 outputPoint = _axis.magnitude > _deadZone ? _axis : Vector2.zero;
                float magnitude = outputPoint.magnitude;

                outputPoint *= outputCurve.Evaluate(magnitude);

                return outputPoint;
            }
            set { SetAxis(value); }
        }

        RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = (JoystickGroup ?? transform as RectTransform);

                return _rectTransform;
            }
        }

        enum ControlType
        {
            ByDrag,
            ByHotkey,
        }
        bool _isDragging;
        int _draggingPointerId;
        bool _isControlledByHotkey;
        bool _IsControlled
        {
            get { return _isDragging || _isControlledByHotkey; }
        }

        [HideInInspector]
        bool dontCallEvent;

        KeyCode _hotkeyLeft = KeyCode.None;
        KeyCode _hotkeyUp = KeyCode.None;
        KeyCode _hotkeyRight = KeyCode.None;
        KeyCode _hotkeyDown = KeyCode.None;
        EventModifiers _hotkeyModifier = EventModifiers.None;

        private float m_dragLength = 0;

        public void SetHotkey(KeyCode left, KeyCode up, KeyCode right, KeyCode down, EventModifiers modifier)
        {
            _hotkeyLeft = left;
            _hotkeyUp = up;
            _hotkeyRight = right;
            _hotkeyDown = down;
            _hotkeyModifier = modifier;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive())
                return;

            if (_isDragging || _isControlledByHotkey)
                return;
            Vector3 pos = eventData.position;
            if (IsRelocation && JoystickGroup != null)
            {

                if (eventData.pressEventCamera != null)
                {
                    pos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
                    JoystickGroup.localPosition = JoystickGroup.parent.InverseTransformPoint(pos);
                }
                else
                {
                    JoystickGroup.localPosition = JoystickGroup.parent.InverseTransformPoint(eventData.position);
                }
            }

            EventSystem.current.SetSelectedGameObject(gameObject, eventData);

            Vector2 newAxis = rectTransform.InverseTransformPoint(pos);
            newAxis.x /= rectTransform.sizeDelta.x * .5f;
            newAxis.y /= rectTransform.sizeDelta.y * .5f;

            SetAxis(newAxis);

            _draggingPointerId = eventData.pointerId;
            dontCallEvent = true;
            ProcessBeginControl(ControlType.ByDrag);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isDragging || _isControlledByHotkey)
                return;

            if (eventData.pointerId != _draggingPointerId)
                return;

            if(IsAutoOrigin && JoystickGroup != null)
            {
                JoystickGroup.localPosition = new Vector3(OriginX, OriginY, 0);
            }

            ProcessEndControl();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!IsActive())
                return;

            eventData.useDragThreshold = false;
        }

        private void ProcessBeginControl(ControlType controlType)
        {
            if (controlType == ControlType.ByDrag)
                _isDragging = true;
            else if (controlType == ControlType.ByHotkey)
            {
                _isControlledByHotkey = true;
            }

            onPress.Invoke(true);
            if (_isAutoHide && JoystickGroup != null)
            {
                JoystickGroup.gameObject.SetActive(true);
                JoystickBack.gameObject.SetActive(false);
            }
        }

        private void ProcessEndControl()
        {
            _isDragging = false;
            _isControlledByHotkey = false;

            onPress.Invoke(false);
            if (_isAutoHide && JoystickGroup != null)
            {
                JoystickGroup.gameObject.SetActive(false);
                JoystickGroup.gameObject.transform.localPosition = JoystickBack.gameObject.transform.localPosition;
                JoystickBack.gameObject.SetActive(true);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _isControlledByHotkey)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(JoystickGroup ?? rectTransform, eventData.position, eventData.pressEventCamera, out _axis);

            // by jp 删除轻操作的慢速移动，统一移动
            //_axis.x /= rectTransform.sizeDelta.x * .5f;
            //_axis.y /= rectTransform.sizeDelta.y * .5f;

            SetAxis(_axis);

            dontCallEvent = true;
        }

        void OnDeselect()
        {
            _isDragging = false;
            _isControlledByHotkey = false;
        }

        bool CheckKeyMofidier(EventModifiers modifier)
        {
            if (modifier == EventModifiers.None)
                return true;

            if ((modifier & EventModifiers.Control) != 0)
                if (!(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    return false;

            if ((modifier & EventModifiers.Alt) != 0)
                if (!(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
                    return false;

            if ((modifier & EventModifiers.Shift) != 0)
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                    return false;

            return true;
        }

        void UpdateHotkey()
        {
            if (_isDragging)
                return;

            if (CheckKeyMofidier(_hotkeyModifier))
            {
                bool bLeft = _hotkeyLeft != KeyCode.None && Input.GetKey(_hotkeyLeft);
                bool bUp = _hotkeyUp != KeyCode.None && Input.GetKey(_hotkeyUp);
                bool bRight = _hotkeyRight != KeyCode.None && Input.GetKey(_hotkeyRight);
                bool bDown = _hotkeyDown != KeyCode.None && Input.GetKey(_hotkeyDown);

                bool bAnykey = bLeft || bUp || bRight || bDown;
                if (_isControlledByHotkey)
                {
                    if (!bAnykey)
                    {
                        ProcessEndControl();
                        dontCallEvent = true;
                        SetAxis(new Vector2(0, 0));
                    }
                }
                else
                {
                    if (bAnykey)
                        ProcessBeginControl(ControlType.ByHotkey);
                }

                if (_isControlledByHotkey)
                {
                    int dx = (bLeft ? -1 : 0) + (bRight ? 1 : 0);
                    int dy = (bUp ? 1 : 0) + (bDown ? -1 : 0);
                    SetAxis(new Vector2(dx, dy));
                    dontCallEvent = true;
                }
            }
        }

        void Update()
        {
            SetHotkey(KeyCode.A, KeyCode.W, KeyCode.D, KeyCode.S, 0);
            UpdateHotkey();

            if (_IsControlled)
                if (!dontCallEvent)
                    if (onValueChange != null) onValueChange.Invoke(JoystickAxis);
        }

        void LateUpdate()
        {
            if (!_IsControlled)
                if (_axis != Vector2.zero)
                {
                    Vector2 newAxis = _axis - (_axis * Time.unscaledDeltaTime * _spring);

                    if (newAxis.sqrMagnitude <= .0001f)
                        newAxis = Vector2.zero;

                    SetAxis(newAxis);
                }

            dontCallEvent = false;
        }

        void OnEnable()
        {
            if (_forceShow == false)
            {
                ProcessEndControl();
            }
        }
        void OnDisable()
        {
            if (_forceShow == false)
            {
                ProcessEndControl();
            }
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
           // UpdateJoystickGraphic();
        }
#endif

        public void SetAxis(Vector2 axis)
        {
            m_dragLength = _axis.magnitude;
            _axis = Vector2.ClampMagnitude(axis, 1);

            Vector2 outputPoint = _axis.magnitude > _deadZone ? _axis : Vector2.zero;
            float magnitude = outputPoint.magnitude;

            outputPoint *= outputCurve.Evaluate(magnitude);

            if (!dontCallEvent)
                if (onValueChange != null)
                    onValueChange.Invoke(outputPoint);

            UpdateJoystickGraphic();
        }


        // 编辑下会初始化2次 2个窗口
        void initLineObj(string _name,ref GameObject _go, ref RectTransform _go_rt)
        {
            _go = new GameObject(_name);
            _go.SetActive(false);
            _go_rt = _go.AddComponent<RectTransform>();
            _go_rt.pivot = new Vector2(0, 0.5f);
            _go_rt.localScale = Vector3.one;
            Image lineObjImg = _go.AddComponent<Image>();
            lineObjImg.color = lineColor;
            lineObjImg.sprite = LineTexture;

            lineObjImg.raycastTarget = false;
            _go_rt.SetParent(JoystickGraphic, false);
        }
        void SetLine(Vector2 _left, Vector2 _end, ref  RectTransform objrt, ref GameObject _obj)
        {

            objrt.localPosition = _left;


            Vector2 t = _end;

            Vector2 t2 = new Vector2(_joystickGraphic.localPosition.x , _joystickGraphic.localPosition.y) + _left;
            t -= t2;

            Vector2 durationPos = new Vector2(t.x, t.y);
            objrt.sizeDelta = new Vector2(durationPos.magnitude, lineWidth);
            float angle = Mathf.Atan2(durationPos.y, durationPos.x) * Mathf.Rad2Deg;
            objrt.localRotation = Quaternion.Euler(0, 0, angle);
            _obj.SetActive(true);
        }


        void UpdateJoystickGraphic()
		{


		    if (_joystickGraphic)
		    {
                //圆形暂时用rectTransform.sizeDelta.x*0.5f作为半径
                _joystickGraphic.localPosition = _axis * Mathf.Min(m_dragLength, rectTransform.sizeDelta.x*0.5f);
		    }

		    // 初始化
            if (lineObj1 == null)
            {
                initLineObj("LineObj1", ref lineObj1, ref lineObjRT1);
            }
            if (lineObj2 == null)
            {
                initLineObj("LineObj2", ref lineObj2, ref lineObjRT2);
            }
            if (lineObj3 == null)
            {
                initLineObj("LineObj3", ref lineObj3, ref lineObjRT3);
            }
            if (lineObj4 == null)
            {
                initLineObj("LineObj4", ref lineObj4, ref lineObjRT4);
            }
            if (lineObj5 == null)
            {
                initLineObj("LineObj5", ref lineObj5, ref lineObjRT5);
            }
            if (lineObj6 == null)
            {
                initLineObj("LineObj6", ref lineObj6, ref lineObjRT6);
            }
            if (lineObj7 == null)
            {
                initLineObj("LineObj7", ref lineObj7, ref lineObjRT7);
            }
            if (lineObj8 == null)
            {
                initLineObj("LineObj8", ref lineObj8, ref lineObjRT8);
            }


           // if (_axis.magnitude != 0)
            {
                // 左边的点
                Vector2 _left = new Vector2(_joystickGraphic.sizeDelta.x * .5f * -1, 0);
               

                   // x =  + radius * cos(angle * 3.14 / 180)

                //  y =  + radius * sin(angle * 3.14 / 180);
                float _r = Mathf.Max(_joystickBackGround.sizeDelta.x, _joystickBackGround.sizeDelta.y) * .5f + DeltaR;

                Vector2 _end1 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f + 90)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f + 90)) * _r);

                Vector2 _end2 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f * 2 + 90)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f * 2 + 90)) * _r);

                Vector2 _end3 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f * 3 + 90)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f * 3 + 90)) * _r);

                Vector2 _end4 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f * 4 + 90)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f * 4 + 90)) * _r);

                SetLine(_left, _end1, ref lineObjRT1,ref lineObj1);
                SetLine(_left, _end2, ref lineObjRT2,ref lineObj2);
                SetLine(_left, _end3, ref lineObjRT3,ref lineObj3);
                SetLine(_left, _end4, ref lineObjRT4,ref lineObj4);


                //右边的点
                Vector2 _right = new Vector2(_joystickGraphic.sizeDelta.x * .5f , 0);

                Vector2 _end5 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f )*0.5f) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f) * 0.5f) * _r);

                Vector2 _end6 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f * 1.5f)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f * 1.5f)) * _r);

                Vector2 _end7 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f *0.5f)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f * -0.5f)) * _r);

                Vector2 _end8 = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (180.0f / 5.0f * -1.5f)) * _r, Mathf.Sin(Mathf.Deg2Rad * (180.0f / 5.0f * -1.5f)) * _r);

                SetLine(_right, _end5, ref lineObjRT5,ref lineObj5);
                SetLine(_right, _end6, ref lineObjRT6,ref lineObj6);
                SetLine(_right, _end7, ref lineObjRT7,ref lineObj7);
                SetLine(_right, _end8, ref lineObjRT8,ref lineObj8);
            }
          // else
          // {
          //     //GameObject.DestroyObject(lineObj);
          //     lineObj1.SetActive(false);
          //     lineObj2.SetActive(false);
          //     lineObj3.SetActive(false);
          //     lineObj4.SetActive(false);
          //     lineObj5.SetActive(false);
          //     lineObj6.SetActive(false);
          //     lineObj7.SetActive(false);
          //     lineObj8.SetActive(false);
          // }
        }

		[System.Serializable]
		public class JoystickMoveEvent : UnityEvent<Vector2> { }

		[System.Serializable]
		public class JoystickPressEvent : UnityEvent<bool> { }
	}
}

#if UNITY_EDITOR
static class JoystickGameObjectCreator
{
	[MenuItem("GameObject/UI/VirtualJoystick")]
	static void Create()
	{
		GameObject go = new GameObject("Joystick", typeof(VirtualJoystick));

		Canvas canvas = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Canvas>() : null;

		Selection.activeGameObject = go;

		if (!canvas)
			canvas = UnityEngine.Object.FindObjectOfType<Canvas>();

		if (!canvas)
		{
			canvas = new GameObject("Canvas", typeof(Canvas), typeof(RectTransform), typeof(GraphicRaycaster)).GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		if (canvas)
			go.transform.SetParent(canvas.transform, false);

		GameObject joystickGroup = new GameObject("JoystickGroup", typeof(RectTransform));
		joystickGroup.transform.SetParent(go.transform, false);

		GameObject background = new GameObject("Background", typeof(Image));
		GameObject graphic = new GameObject("Graphic", typeof(Image));

		background.transform.SetParent(joystickGroup.transform, false);
		graphic.transform.SetParent(joystickGroup.transform, false);

		background.GetComponent<Image>().color = new Color(1, 1, 1, .86f);

		RectTransform backgroundTransform = graphic.transform as RectTransform;
		RectTransform graphicTransform = graphic.transform as RectTransform;

		graphicTransform.sizeDelta = backgroundTransform.sizeDelta * .5f;

		VirtualJoystick joystick = go.GetComponent<VirtualJoystick>();
		joystick.JoystickGraphic = graphicTransform;
		joystick.JoystickGroup = joystickGroup.GetComponent<RectTransform>();

        joystick.JoystickBackGround = background.GetComponent<RectTransform>();
        //joystick.JoystickBackGround;
    }
}
#endif