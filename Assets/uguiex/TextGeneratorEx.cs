using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Text;

namespace UnityEngineEx
{
	/// <summary>
	/// Replace and improve TextGenerator:
	/// 1 Fix CJK text line break algorithm
	/// 2 Add escape character for rich text
	///  use @"\<" when a plain '<' needed; @"\n\t" are also supported
	/// New features can be disabled if legacyTextGenerator in constructor params is true
	/// </summary>
	public sealed class TextGeneratorEx : IDisposable
	{
		private bool m_isLegacyTextGenerator;
		private TextGenerator m_generator;

		// Summary:
		//     Create a TextGenerator.
		//
		// Parameters:
		//   extented: whether change default TextGenerator behaviour
		//   initialCapacity:
		public TextGeneratorEx(bool legacyTextGenerator)
		{
			//Debug.LogWarningFormat("create TextGeneratorEx extented: {0}", extented);
			m_isLegacyTextGenerator = legacyTextGenerator;
			m_generator = new TextGenerator();
		}

		//
		// Summary:
		//     Create a TextGenerator.
		//
		// Parameters:
		//   extented: whether change default TextGenerator behaviour
		//   initialCapacity:
		public TextGeneratorEx(bool legacyTextGenerator, int initialCapacity)
		{
			m_isLegacyTextGenerator = legacyTextGenerator;
			m_generator = new TextGenerator(initialCapacity);
		}

		/// <summary>
		/// char that stands for soft line break
		/// </summary>
		public const char SoftLineBreakChar = '\x0b';

		/// <summary>
		/// the internal string to send to TextGenerator
		/// could insert soft line break char
		/// </summary>
		public string LastInternalString
		{
			get { return m_LastInternalString; }
		}

		// Summary:
		//     The number of characters that have been generated.
		public int characterCount
		{
			get
			{
				return m_generator.characterCount;
			}
		}
		//
		// Summary:
		//     The number of characters that have been generated and are included in the
		//     visible lines.
		public int characterCountVisible
		{
			get
			{
				return m_generator.characterCountVisible;
			}
		}

		//
		// Summary:
		//     Array of generated characters.
		public IList<UICharInfo> characters
		{
			get
			{
				return m_generator.characters;
			}
		}
		//
		// Summary:
		//     The size of the font that was found if using best fit mode.
		public int fontSizeUsedForBestFit
		{
			get
			{
				if (m_isLegacyTextGenerator)
					return m_generator.fontSizeUsedForBestFit;

				return m_fontSizeUsedForBestFit;
			}
		}

		//
		// Summary:
		//     Number of text lines generated.
		public int lineCount
		{
			get
			{
				return m_generator.lineCount;
			}
		}

		//
		// Summary:
		//     Information about each generated text line.
		public IList<UILineInfo> lines
		{
			get
			{
				return m_generator.lines;
			}
		}

		//
		// Summary:
		//     Extents of the generated text in rect format.
		public Rect rectExtents
		{
			get
			{
				return m_generator.rectExtents;
			}
		}

		//
		// Summary:
		//     Number of vertices generated.
		public int vertexCount
		{
			get
			{
				return m_generator.vertexCount;
			}
		}
		//
		// Summary:
		//     Array of generated vertices.
		public IList<UIVertex> verts
		{
			get
			{
				return m_generator.verts;
			}
		}

		public void GetCharacters(List<UICharInfo> characters)
		{
			m_generator.GetCharacters(characters);
		}

		//
		// Summary:
		//     Returns the current UICharInfo.
		//
		// Returns:
		//     Character information.
		public UICharInfo[] GetCharactersArray()
		{
			return m_generator.GetCharactersArray();
		}

		public void GetLines(List<UILineInfo> lines)
		{
			m_generator.GetLines(lines);
		}

		//
		// Summary:
		//     Returns the current UILineInfo.
		//
		// Returns:
		//     Line information.
		public UILineInfo[] GetLinesArray()
		{
			return m_generator.GetLinesArray();
		}

		//
		// Summary:
		//     Given a string and settings, returns the preferred height for a container
		//     that would hold this text.
		//
		// Parameters:
		//   str:
		//     Generation text.
		//
		//   settings:
		//     Settings for generation.
		//
		// Returns:
		//     Preferred height.
		public float GetPreferredHeight(string str, TextGenerationSettings settings)
		{
			if (m_isLegacyTextGenerator)
				return m_generator.GetPreferredHeight(str, settings);

			String internalStr = settings.richText ? EscapeRichText(str, settings) : str;
			return m_generator.GetPreferredHeight(internalStr, settings);
		}

		//
		// Summary:
		//     Given a string and settings, returns the preferred width for a container
		//     that would hold this text.
		//
		// Parameters:
		//   str:
		//     Generation text.
		//
		//   settings:
		//     Settings for generation.
		//
		// Returns:
		//     Preferred width.
		public float GetPreferredWidth(string str, TextGenerationSettings settings)
		{
			if (m_isLegacyTextGenerator)
				return m_generator.GetPreferredWidth(str, settings);

			String internalStr = settings.richText ? EscapeRichText(str, settings) : str;
			return m_generator.GetPreferredWidth(internalStr, settings);
		}

		public void GetVertices(List<UIVertex> vertices)
		{
			m_generator.GetVertices(vertices);
		}

		//
		// Summary:
		//     Returns the current UILineInfo.
		//
		// Returns:
		//     Vertices.
		public UIVertex[] GetVerticesArray()
		{
			return m_generator.GetVerticesArray();
		}

		//
		// Summary:
		//     Mark the text generator as invalid. This will force a full text generation
		//     the next time Populate is called.
		public void Invalidate()
		{
			m_generator.Invalidate();

			this.m_HasGenerated = false;
		}

		//
		// Summary:
		//     Will generate the vertices and other data for the given string with the given
		//     settings.
		//
		// Parameters:
		//   str:
		//     String to generate.
		//
		//   settings:
		//     Settings.
		public bool Populate(string str, TextGenerationSettings settings)
		{
			if (m_isLegacyTextGenerator)
			{
				m_LastInternalString = str;
				return m_generator.Populate(str, settings);
			}

			if (this.m_HasGenerated && str == this.m_LastString && settings.Equals (this.m_LastSettings))
			{
				return this.m_LastValid;
			}
			return this.PopulateAlways (str, settings);
		}

		void IDisposable.Dispose()
		{
			(m_generator as IDisposable).Dispose();
		}

		bool PopulateAlways(string str, TextGenerationSettings settings)
		{
			this.m_LastString = str;
			this.m_HasGenerated = true;
			this.m_LastSettings = settings;
			m_LastValid = this.PopulateInternal(str, settings);
			return m_LastValid;
		}

		bool PopulateInternal(string str, TextGenerationSettings settings)
		{
			TextGenerationSettings internalSettings = settings;
			Int32 fitFontSize;
			string internalStr = WrapAndBestFitText(str, ref internalSettings, out fitFontSize);
			m_LastInternalString = internalStr;

			String internalStrForPopulate = internalStr.Replace(SoftLineBreakChar, '\n');
			//Debug.LogFormat("m_generator.Populate, fontSize: {0}, str: {1}", internalSettings.fontSize, internalStrForPopulate);
			bool popuateResult = m_generator.Populate(internalStrForPopulate, internalSettings);
			m_fontSizeUsedForBestFit = fitFontSize < 0 ? m_generator.fontSizeUsedForBestFit : fitFontSize;	//minus value means should use TextGenerator result
			return popuateResult;
		}

		/// <summary>
		/// wrap text and auto resize font size to best fit. could change setting
		/// </summary>
		/// <param name="str"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		String WrapAndBestFitText(string str, ref TextGenerationSettings settings, out Int32 fitFontSize)
		{
			if (!settings.resizeTextForBestFit || Math.Floor(settings.generationExtents.y) == 0)		//no resize
			{
				fitFontSize = -1;	//use TextGenerator result
				return WrapText(str, settings.fontSize, ref settings);
			}

			if (settings.horizontalOverflow != HorizontalWrapMode.Wrap)		//no wrap, default best fit is ok, better performance
			{
				fitFontSize = -1;	//use TextGenerator result
				return WrapText(str, settings.fontSize, ref settings);
			}

			String result = WrapAndBestFitTextInternal(str, ref settings, out fitFontSize);
			settings.resizeTextForBestFit = false;
			settings.fontSize = fitFontSize;
			//Debug.LogWarningFormat("WrapAndBestFitText result, fitFontSize: {0}, scale: {1}, text: {2}", fitFontSize, settings.scaleFactor, result);
			return result;
		}

		struct BestFitTryInfo
		{
			public String str;
			public Int32 fontSize;
			public TextGenerationSettings settings;

			public BestFitTryInfo(String str, Int32 fontSize, TextGenerationSettings settings)
			{
				this.str = str;
				this.fontSize = fontSize;
				this.settings = settings;
			}
		}

		Single TestHeight(String str, Int32 fontSize, TextGenerationSettings settings)
		{
			settings.fontSize = fontSize;
			settings.resizeTextForBestFit = false;
			settings.horizontalOverflow = HorizontalWrapMode.Overflow;

			float height = m_generator.GetPreferredHeight(str.Replace(SoftLineBreakChar, '\n'), settings);
			//Debug.LogWarningFormat("TestHeight, fontSize: {0}, scale: {1}, result: {2}, text: {3}", fontSize, settings.scaleFactor, height, str);
			return height;
		}

		//return true means can guess more
		/// <summary>
		/// user should record new result, last result and last good result, when return false:
		/// if new result is good, take it; else if there is last good result, take it; else take the new result
		/// </summary>
		Boolean GuessNextFitFontSize(Int32 lastFontSize, Single lastHeight, Boolean lastFit, Int32 heightLimit, ref Int32 lowLimit, ref Int32 highLimit, out Int32 nextFontSize)
		{
			//binary search for lower boundary, predicator is lastFit
			
			//detect limit
			if (lastFit && lastFontSize >= highLimit || !lastFit && lastFontSize <= lowLimit)
			{
				nextFontSize = lastFontSize;
				return false;
			}
			//now can move at least 1

			//update limit
			if (lastFit)
				lowLimit = Math.Max(lowLimit, lastFontSize) + 1;
			else
				highLimit = Math.Min(highLimit, lastFontSize) - 1;

			// guess next value
			// size^2 ~ width * height
			Double factor = (Double)(heightLimit / lastHeight);
			Int32 guessFontSize = (Int32)(lastFontSize * Math.Sqrt(factor));

			//fix next value
			nextFontSize = guessFontSize;
			if (lastFit)	//increase
			{
				nextFontSize = Math.Min(Math.Max(nextFontSize, lastFontSize+1), highLimit);
			}
			else	//decrease
			{
				nextFontSize = Math.Max(Math.Min(nextFontSize, lastFontSize-1), lowLimit);
			}
			
			return nextFontSize != lastFontSize;
		}

		String WrapAndBestFitTextInternal(string str, ref TextGenerationSettings settings, out Int32 fitFontSize)
		{
			if (settings.resizeTextMinSize > settings.resizeTextMaxSize)	//unreasonable suituation, keep same as TextGenerator
			{
				fitFontSize = settings.resizeTextMinSize;
				return WrapText(str, settings.resizeTextMinSize, ref settings);
			}

			Int32 heightLimit = (Int32)Math.Floor(settings.generationExtents.y);

			Int32 lastFontSize = Math.Min(Math.Max(settings.fontSize, settings.resizeTextMinSize), settings.resizeTextMaxSize);
			TextGenerationSettings lastSetting = settings;
			String lastStr = WrapText(str, lastFontSize, ref lastSetting);
			Single lastHeight = TestHeight(lastStr, lastFontSize, lastSetting);
			Boolean lastFit = lastHeight <= heightLimit;

			//Debug.LogWarningFormat("WrapAndBestFitText first try size: {0}, Height: {1}, limit: {2}, fit: {3}, str: {4}", lastFontSize, lastHeight, heightLimit, lastFit, lastStr);
			
			Int32 lastGoodFontSize = lastFontSize;
			TextGenerationSettings lastGoodSetting = lastSetting;
			String lastGoodStr = lastStr;
			Single lastGoodHeight = lastHeight;
			Boolean hasLastGood = lastFit;
			
			//best fit font size is between lowLimit and highLimit
			Int32 lowLimit = settings.resizeTextMinSize;
			Int32 highLimit = settings.resizeTextMaxSize;

			//try font size until best fit
			while (true)
			{
				Int32 newFontSize;
				Boolean needGuessMore = GuessNextFitFontSize(lastFontSize, lastHeight, lastFit, heightLimit, ref lowLimit, ref highLimit, out newFontSize);

				if (newFontSize == lastFontSize)
				{
					//Debug.LogWarningFormat("Unable to resize more from, lastFontSize: {0}, lastFit: {1}, lowLimit: {2}, highLimit: {3}", lastFontSize, lastFit, lowLimit, highLimit);
					if (hasLastGood)		//take last good result
					{
						settings = lastGoodSetting;
						fitFontSize = lastGoodFontSize;
						return lastGoodStr;
					}
					else		//take last result
					{
						settings = lastSetting;
						fitFontSize = lastFontSize;
						return lastStr;
					}
				}

				TextGenerationSettings newSetting = settings;
				String newStr = WrapText(str, newFontSize, ref newSetting);
				float newHeight = TestHeight(newStr, newFontSize, newSetting);
				Boolean newFit = newHeight <= heightLimit;
				//Debug.LogWarningFormat("WrapAndBestFitText try size: {0}, Height: {1}, limit: {2}, fit: {3}, str: {4}", newFontSize, newHeight, heightLimit, newFit, newStr);

				if (needGuessMore)
				{
					lastFit = newFit;
					lastFontSize = newFontSize;
					lastSetting = newSetting;
					lastStr = newStr;
					lastHeight = newHeight;

					if (lastFit)
					{
						hasLastGood = true;
						lastGoodFontSize = newFontSize;
						lastGoodSetting = newSetting;
						lastGoodStr = newStr;
						lastGoodHeight = newHeight;
					}
				}
				else	//finish
				{
					if (hasLastGood)		//take last good result
					{
						//Debug.LogWarningFormat("Finished, take last good result");
						settings = lastGoodSetting;
						fitFontSize = lastGoodFontSize;
						return lastGoodStr;
					}
					else		//take the new result
					{
						//Debug.LogWarningFormat("Finished, take the new result");
						settings = newSetting;
						fitFontSize = newFontSize;
						return newStr;
					}
				}
			}
		}

		/// <summary>
		/// wrap text according to given setting. could change setting
		/// </summary>
		/// <param name="str"></param>
		/// <param name="fitFontSize"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		String WrapText(string str, int fitFontSize, ref TextGenerationSettings settings)
		{
			if (settings.horizontalOverflow == HorizontalWrapMode.Wrap)
			{
				String result;
				if (settings.richText)
				{
					result = WrapAndEscapeRichText(str, fitFontSize, settings);
				}
				else
				{
					result = WrapPlainText(str, fitFontSize, settings);
				}
				settings.horizontalOverflow = HorizontalWrapMode.Overflow;
				return result;
			}
			else
			{
				String result;
				if (settings.richText)
				    result = EscapeRichText(str, settings);
				//if (settings.richText)
				//{
				//    TextGenerationSettings internalSettings = settings;
				//    internalSettings.generationExtents = new UnityEngine.Vector2(1e9f, 0);
				//    result = WrapAndEscapeRichText(str, fitFontSize, internalSettings);
				//}
				else
					result = str;

				return result;
			}
		}

		List<RichTextItem> m_plainTextList = null;
		String WrapPlainText(string str, int fitFontSize, TextGenerationSettings settings)
		{
			RichTextItem item = new RichTextItem(new RichTextHelper.Substring(str, 0, str.Length), true, settings.fontStyle, settings.fontSize);
			item.text = new RichTextHelper.Substring(str, 0, str.Length);
			if (m_plainTextList == null)
			{
				m_plainTextList = new List<RichTextItem>();
				m_plainTextList.Add(new RichTextItem());
			}
			m_plainTextList[0] = item;

			StringBuilder strBuilder = new StringBuilder();
			WrapTextItems(m_plainTextList, fitFontSize, settings, strBuilder);
			return strBuilder.ToString();
		}

		StringBuilder m_EscapeRichTextStrBuilder = null;
		String EscapeRichText(string richText, TextGenerationSettings settings)
		{
			// reuse string buffer
			if (m_EscapeRichTextStrBuilder == null)
				m_EscapeRichTextStrBuilder = new StringBuilder();
			m_EscapeRichTextStrBuilder.Remove(0, m_EscapeRichTextStrBuilder.Length);

			StringBuilder strBuilder = m_EscapeRichTextStrBuilder;
			foreach (RichTextItem textItem in EscapeAndExtractRichText(richText, false, settings))
			{
				strBuilder.Append(textItem.text.WholeString, textItem.text.Begin, textItem.text.Length);
			}
			return strBuilder.ToString();
		}

		StringBuilder m_WrapAndEscapeRichTextStrBuilder = null;
		String WrapAndEscapeRichText(string richText, int fitFontSize, TextGenerationSettings settings)
		{
			// reuse string buffer
			if (m_WrapAndEscapeRichTextStrBuilder == null)
				m_WrapAndEscapeRichTextStrBuilder = new StringBuilder();
			m_WrapAndEscapeRichTextStrBuilder.Remove(0, m_WrapAndEscapeRichTextStrBuilder.Length);

			StringBuilder strBuilder = m_WrapAndEscapeRichTextStrBuilder;
			WrapTextItems(EscapeAndExtractRichText(richText, true, settings), fitFontSize, settings, strBuilder);
			return strBuilder.ToString();
		}

		/// <summary>
		/// escape and wrap given rich text. enumerator return each piece of text
		/// </summary>
		/// <param name="richText"></param>
		/// <param name="doWrap"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		IEnumerable<RichTextItem> EscapeAndExtractRichText(string richText, bool doWrap, TextGenerationSettings settings)
		{
			bool fontSettingIsBold = settings.fontStyle == FontStyle.Bold || settings.fontStyle == FontStyle.BoldAndItalic;
			bool fontSettingIsItalic = settings.fontStyle == FontStyle.Italic || settings.fontStyle == FontStyle.BoldAndItalic;

			foreach (RichTextHelper.RichTextSpanInfo span in RichTextHelper.ParseRichText(richText, fontSettingIsBold, fontSettingIsItalic))
			{
				RichTextHelper.Substring text = span.text;
				Boolean isVisible = span.isVisible;
				Boolean isBold = span.isBold;
				Boolean isItalic = span.isItalic;
				Int32 size = span.size;
				FontStyle style = isBold ? (isItalic ? FontStyle.BoldAndItalic : FontStyle.Bold) : (isItalic ? FontStyle.Italic : FontStyle.Normal);

				if (isVisible)
				{
					Int32 lastNormalBegin = text.Begin;
					while (true)
					{
						//replace '<' with "<material><</material>"
						Int32 ltIndex = text.WholeString.IndexOf('<', lastNormalBegin, text.End-lastNormalBegin);
						if (ltIndex >= 0)
						{
							yield return new RichTextItem(new RichTextHelper.Substring(text.WholeString, lastNormalBegin, ltIndex-lastNormalBegin), true, style, size);
							yield return new RichTextItem(new RichTextHelper.Substring("<material>"), false, FontStyle.Normal, 0);
							yield return new RichTextItem(new RichTextHelper.Substring("<"), true, style, size);
							yield return new RichTextItem(new RichTextHelper.Substring("</material>"), false, FontStyle.Normal, 0);
							lastNormalBegin = ltIndex + 1;
						}
						else
						{
							break;
						}
					}
					if (lastNormalBegin < text.End)
						yield return new RichTextItem(new RichTextHelper.Substring(text.WholeString, lastNormalBegin, text.End-lastNormalBegin), true, style, size);
				}
				else
				{
					yield return new RichTextItem(text, false, FontStyle.Normal, 0);
				}
			}
		}

		struct RichTextItem
		{
			public RichTextHelper.Substring text;
			public Boolean visible;
			public FontStyle style;
			public Int32 size;

			public RichTextItem(RichTextHelper.Substring text, Boolean visible, FontStyle style, Int32 size)
			{
				this.text = text;
				this.visible = visible;
				this.style = style;
				this.size = size;
			}
		}

		static int CalculateResizedFontSize(int tagSize, int fitFontSize, int fontSize, int fitMaxFontSize)
		{
			if (tagSize < 0)	//no size tag, can scale freely
			{
				return fitFontSize;
			}
			else
			{
				if (fitFontSize >= fontSize)	//font enlarged, scale taged text linearly
				{
					return tagSize*fitFontSize/fontSize;
				}
				else	//font shrink, size/fitSize = tagSize/min(fontSize, fitMaxFontSize), but size can not beyond tagSize
				{
					int size = fitFontSize*tagSize/Math.Min(fontSize, fitMaxFontSize);
					return Math.Min(tagSize, size);
				}
			}
		}

		/// <summary>
		/// wrap text items, write final text into strBuilder
		/// </summary>
		/// <param name="items"></param>
		/// <param name="settings"></param>
		/// <param name="strBuilder">hold the wrapped text</param>
		void WrapTextItems(IEnumerable<RichTextItem> items, int fitFontSize, TextGenerationSettings settings, StringBuilder strBuilder)
		{
			Int32 wrapWidth = (Int32)Math.Floor(settings.generationExtents.x);
			Font font = settings.font;
			//Debug.LogWarningFormat("WrapTextItems, {0}, {1}", fontSize, fitFontSize);

			Int32 currentX = 0;
			Int32 lastBreakOpportunityBufIndex = -1;
			Int32 lastBreakOpportunityX = 0;		//valid when lastBreakOpportunityBufIndex >= 0. after opportunity, skipping following spaces
			Char prevChar = '\n';
			Boolean replaceSizeTag = false;

			Boolean justAfterBreakOpportunity = false;
			Boolean justAfterSoftLineBreak = false;

			foreach (RichTextItem item in items)
			{
				//Debug.LogFormat("RichTextItem, visible: {0}, style: {1}, size: {2}, text: {3}", item.visible, item.style, item.size, item.text);
				if (!item.visible)
				{
					if (String.Compare(item.text.WholeString, item.text.Begin, "<size=", 0, "<size=".Length) == 0)	//need replace this size tag
						replaceSizeTag = true;
					else
						strBuilder.Append(item.text);
					continue;
				}

				Int32 itemFontSize = CalculateResizedFontSize(item.size, fitFontSize, settings.fontSize, settings.resizeTextMaxSize);

				if (replaceSizeTag)
				{
					replaceSizeTag = false;

					//Debug.Log("replace tag size to: " +itemFontSize);
					strBuilder.Append("<size=");
					strBuilder.Append(itemFontSize.ToString());
					strBuilder.Append(">");
				}

				font.RequestCharactersInTexture(item.text.ToString(), itemFontSize, item.style);
				for (Int32 i=0; i<item.text.Length; ++i)
				{
					Char ch = item.text.At(i);
					//Debug.LogFormat("ch: {0}", ch);

					justAfterBreakOpportunity = justAfterBreakOpportunity && ch == ' ';
					justAfterSoftLineBreak = justAfterSoftLineBreak && ch == ' ';

					if (ch == '\n')		//hard line breaker
					{
						strBuilder.Append('\n');
						currentX = 0;
						lastBreakOpportunityBufIndex = -1;
						prevChar = '\n';
						//Debug.LogFormat("hard line break");
						continue;
					}

					//ignore all space width after soft line break
					if (justAfterSoftLineBreak && ch == ' ')
					{
						//Debug.Log("swallow space after soft line break");
						strBuilder.Append(ch);
						prevChar = ch;
						continue;
					}

					//append current char
					CharacterInfo chInfo;
					font.GetCharacterInfo(ch, out chInfo, itemFontSize, item.style);
					Int32 newX = currentX + chInfo.advance;
					
					//update break opportunity (between prev and current char)
					if (currentX != 0)
					{
						Boolean canBreak;
						Boolean prevCJK = IsCJKChar(prevChar);
						Boolean curCJK = IsCJKChar(ch);
						if (prevCJK || curCJK)
						{
							canBreak = !IsCJKLeadingPunctuation(prevChar) && !IsCJKTrailingPunctuation(ch);
						}
						else	//both are not CJK
						{
							canBreak = (prevChar == ' ' || ch == ' ');
						}

						if (canBreak)
						{
							lastBreakOpportunityBufIndex = strBuilder.Length;
							lastBreakOpportunityX = currentX;
							justAfterBreakOpportunity = true;
							//Debug.Log("set lastBreakOpportunityX " + lastBreakOpportunityX);
						}
					}

					//spaces after break opportunity should not break line
					if (justAfterBreakOpportunity && ch == ' ')
					{
						currentX = newX;
						lastBreakOpportunityX = newX;
						++lastBreakOpportunityBufIndex;
						strBuilder.Append(ch);
						prevChar = ch;
						//Debug.LogFormat("update lastBreakOpportunityX to index: {0}, x: {1}", lastBreakOpportunityBufIndex, lastBreakOpportunityX);
						continue;
					}

					if (newX > wrapWidth && chInfo.advance > 0 && lastBreakOpportunityBufIndex >= 0)	//need wrap and has break opportunity
					{
						strBuilder.Insert(lastBreakOpportunityBufIndex, SoftLineBreakChar);
						currentX = Math.Max(currentX - lastBreakOpportunityX, 0);
						//Debug.LogFormat("line break on opportunity at index: {0}, new currentX: {1}", lastBreakOpportunityBufIndex, currentX);
						newX = currentX + chInfo.advance;
						lastBreakOpportunityBufIndex = -1;
						if (justAfterBreakOpportunity)
							justAfterSoftLineBreak = true;
					}

					if (currentX != 0 && newX > wrapWidth)	//still need wrap
					{
						//Debug.Log("need wrap when no break opportunity");
						strBuilder.Append(SoftLineBreakChar);
						strBuilder.Append(ch);
						justAfterSoftLineBreak = (ch == ' ');
						if (ch != ' ')
							currentX = chInfo.advance;
					}
					else
					{
						strBuilder.Append(ch);
						currentX = newX;
					}
					//Debug.LogFormat("char {0}, fontsize: {1}, width, {2}, x: {3}", ch, itemFontSize, chInfo.advance, currentX);

					//update prevChar
					prevChar = ch;
				}
			}
		}

		/// <summary>
		/// whether is CJK character. CJK characters can generally break line anywhere
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		static bool IsCJKChar (char ch)
		{
			return ch >= 0x2E80 ||
				(ch >= 2000 && ch <= 206F);		//some CJK punctuations
		}

		/// <summary>
		/// whether is CJK punctuation that can not appear at end of line
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		static bool IsCJKLeadingPunctuation (char ch)
		{
			return "\x2018\x201C\x300C\x300A\xFF08".IndexOf(ch) >= 0;		//@"‘“「《（"
		}

		/// <summary>
		/// whether is CJK punctuation that can not appear at begin of line
		/// </summary>
		/// <param name="ch"></param>
		/// <returns></returns>
		static bool IsCJKTrailingPunctuation (char ch)
		{
			return "\x2019\x201D\x300D\x300B\xFF09\xFF0C\x3002\xFF1A\xFF1B\x3001\xFF1F\xFF01".IndexOf(ch) >= 0;		//@"’”」》），。：；、？！"
		}

		private bool m_HasGenerated;
		private TextGenerationSettings m_LastSettings;
		private string m_LastString;
		private string m_LastInternalString;
		private bool m_LastValid;
		private int m_fontSizeUsedForBestFit;
	}
}
