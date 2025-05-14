namespace UnityEngine.UI
{
	/// <summary>
	/// layout type of LinearLayoutGroup
	/// </summary>
	public enum LinearLayoutOrientation
	{
		/// <summary>
		/// arrange along long side of screen
		/// same as VerticalLayoutGroup on portrait screen, same as HorizontalLayoutGroup on landscape screen
		/// </summary>
		ScreenLongSide = 0,
		/// <summary>
		/// arrange along short side of screen
		/// same as HorizontalLayoutGroupon portrait screen, same as VerticalLayoutGroup on landscape screen
		/// </summary>
		ScreenShortSide = 1,
		/// <summary>
		/// same as HorizontalLayoutGroup
		/// </summary>
		Horizontal = 2,
		/// <summary>
		/// same as VerticalLayoutGroup
		/// </summary>
		Vertial = 3,
	}

    [AddComponentMenu("Layout/Linear Layout Group", 151)]
    public class LinearLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
		[SerializeField]
		private LinearLayoutOrientation m_Orientation;
		public LinearLayoutOrientation Orientation { get { return m_Orientation; } set { SetSplitProperty(ref m_Orientation, value); } }

		[SerializeField]
		private bool m_SplitSetting;
		public bool SplitSetting { get { return m_SplitSetting; } set { SetSplitProperty(ref m_SplitSetting, value); } }

		//split settings
		[SerializeField] private RectOffset m_HorizontalPadding = new RectOffset();
		public RectOffset horizontalPadding { get { return m_HorizontalPadding; } set { SetSplitProperty(ref m_HorizontalPadding, value); } }

		[SerializeField] private RectOffset m_VerticalPadding = new RectOffset();
		public RectOffset verticalPadding { get { return m_VerticalPadding; } set { SetSplitProperty(ref m_VerticalPadding, value); } }

        [SerializeField] protected float m_HorizontalSpacing = 0;
        public float horizontalSpacing { get { return m_HorizontalSpacing; } set { SetSplitProperty(ref m_HorizontalSpacing, value); } }

        [SerializeField] protected float m_VerticalSpacing = 0;
        public float verticalSpacing { get { return m_VerticalSpacing; } set { SetSplitProperty(ref m_VerticalSpacing, value); } }

        [SerializeField] protected TextAnchor m_HorizontalChildAlignment = TextAnchor.UpperLeft;
        public TextAnchor horizontalChildAlignment { get { return m_HorizontalChildAlignment; } set { SetSplitProperty(ref m_HorizontalChildAlignment, value); } }

        [SerializeField] protected TextAnchor m_VerticalChildAlignment = TextAnchor.UpperLeft;
        public TextAnchor verticalChildAlignment { get { return m_VerticalChildAlignment; } set { SetSplitProperty(ref m_VerticalChildAlignment, value); } }
        
		[SerializeField] protected bool m_HorizontalChildForceExpandWidth = true;
        public bool horizontalChildForceExpandWidth { get { return m_HorizontalChildForceExpandWidth; } set { SetSplitProperty(ref m_HorizontalChildForceExpandWidth, value); } }

		[SerializeField] protected bool m_VerticalChildForceExpandWidth = true;
        public bool verticalChildForceExpandWidth { get { return m_VerticalChildForceExpandWidth; } set { SetSplitProperty(ref m_VerticalChildForceExpandWidth, value); } }

        [SerializeField] protected bool m_HorizontalChildForceExpandHeight = true;
        public bool horizontalChildForceExpandHeight { get { return m_HorizontalChildForceExpandHeight; } set { SetSplitProperty(ref m_HorizontalChildForceExpandHeight, value); } }

        [SerializeField] protected bool m_VerticalChildForceExpandHeight = true;
        public bool verticalChildForceExpandHeight { get { return m_VerticalChildForceExpandHeight; } set { SetSplitProperty(ref m_VerticalChildForceExpandHeight, value); } }


        protected LinearLayoutGroup()
        {}

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, IsVertical());
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, IsVertical());
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, IsVertical());
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, IsVertical());
        }

		private LinearLayoutOrientation m_lastOrientation = (LinearLayoutOrientation)(-1);
		public void Update()
		{
			LinearLayoutOrientation currentOrientation = DecideOrientation();
			if (currentOrientation != m_lastOrientation)
			{
				m_lastOrientation = currentOrientation;
				ReapplySetting(currentOrientation);
				SetDirty();
			}
		}

		private LinearLayoutOrientation DecideOrientation()
		{
			switch (m_Orientation)
			{
				case LinearLayoutOrientation.ScreenLongSide:
					return Screen.width > Screen.height ? LinearLayoutOrientation.Horizontal : LinearLayoutOrientation.Vertial;
				case LinearLayoutOrientation.ScreenShortSide:
					return Screen.width > Screen.height ? LinearLayoutOrientation.Vertial : LinearLayoutOrientation.Horizontal;
				default:
					return m_Orientation;
			}
		}

		private bool IsVertical()
		{
			return DecideOrientation() == LinearLayoutOrientation.Vertial;
		}

		private void ReapplySetting(LinearLayoutOrientation currentOrientation)
		{
			if (m_SplitSetting)
			{
				if (currentOrientation == LinearLayoutOrientation.Vertial)
				{
					padding = verticalPadding;
					spacing = verticalSpacing;
					childAlignment = verticalChildAlignment;
					childForceExpandWidth = verticalChildForceExpandWidth;
					childForceExpandHeight = verticalChildForceExpandHeight;
				}
				else
				{
					padding = horizontalPadding;
					spacing = horizontalSpacing;
					childAlignment = horizontalChildAlignment;
					childForceExpandWidth = horizontalChildForceExpandWidth;
					childForceExpandHeight = horizontalChildForceExpandHeight;
				}
			}
		}

		protected void SetSplitProperty<T>(ref T currentValue, T newValue)
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return;
            currentValue = newValue;
			ReapplySetting(DecideOrientation());
            SetDirty();
        }

#if UNITY_EDITOR
		void OnValidate()
		{
			m_lastOrientation = (LinearLayoutOrientation)(-1);
			SetDirty();
		}
#endif
    }
}
