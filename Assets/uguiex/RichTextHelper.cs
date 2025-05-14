using System;
using System.Collections.Generic;

class RichTextHelper
{
	public struct Substring
	{
		public Substring(String wholeString, Int32 begin, Int32 length)
		{
			if (begin < 0 || length < 0 || begin + length > wholeString.Length)
				throw new Exception("invalid range");

			m_wholeString = wholeString;
			m_begin = begin;
			m_length = length;
		}

		public Substring(String str) : this(str, 0, str.Length)
		{
		}

		public String WholeString { get { return m_wholeString; } }
		/// <summary>
		/// index of first character in whole string
		/// </summary>
		public Int32 Begin { get { return m_begin; } }
		/// <summary>
		/// index after last character in whole string
		/// </summary>
		public Int32 End { get { return m_begin + m_length; } }
		public Int32 Length { get { return m_length; } }
		public String Value { get { return m_wholeString.Substring(m_begin, Length); } }
		public Boolean HasValue { get { return m_wholeString != null; } }
		public String NullableValue { get { return HasValue ? Value : null; } }
		/// <summary>
		/// get character at index
		/// </summary>
		/// <param name="index">charactor index</param>
		/// <returns>character at index</returns>
		public Char At(Int32 index)
		{
			return m_wholeString[m_begin + index];
		}

		public Boolean Equals(Substring right)
		{
			if (m_length != right.m_length)
				return false;

			return String.Compare(m_wholeString, m_begin, right.m_wholeString, right.m_begin, right.m_length) == 0;
		}

		public Boolean Equals(String right)
		{
			if (m_length != right.Length)
				return false;

			return String.Compare(m_wholeString, m_begin, right, 0, right.Length) == 0;
		}

		public override String ToString() { return Value; }

		private readonly String m_wholeString;
		private readonly Int32 m_begin;
		private readonly Int32 m_length;
	}

	enum TagType
	{
		None,
		Bold,
		Italic,
		Size,
		Color,
	}

	enum ExtractRichTextTagState
	{
		Normal,
		Escape,			//after '\\'
		TagBegin,		//after '<'
		TagName,
		TagParam,
	}

	struct RichTextTagInfo
	{
		public Substring content;
		public TagType tagType;
		public Boolean isClose;
		/// <summary>
		/// only valid when tagType is Size or Color and not isClose
		/// </summary>
		public Substring tagParam;

		public RichTextTagInfo(Substring content, TagType tagType, Boolean isClose, Substring tagParam)
		{
			this.content = content;
			this.tagType = tagType;
			this.isClose = isClose;
			this.tagParam = tagParam;
		}

		public RichTextTagInfo(Substring content, TagType tagType, Boolean isClose) : this(content, tagType, isClose, new Substring("", 0, 0))
		{
		}
	}

	static TagType TagNameToTagType(Substring tagName)
	{
		if (tagName.Length == 0)
			return TagType.None;

		switch (tagName.At(0))
		{
			case 'i':
				return tagName.Equals("i") ? TagType.Italic : TagType.None;
			case 'b':
				return tagName.Equals("b") ? TagType.Bold : TagType.None;
			case 's':
				return tagName.Equals("size") ? TagType.Size : TagType.None;
			case 'c':
				return tagName.Equals("color") ? TagType.Color : TagType.None;
			default:
				return TagType.None;
		}
	}

	/// <summary>
	///  extract rich text tags, unescape \?
	/// valid tags: <i>, </i>, <b>, </b>, <size=\d*>, </size>, <color=...>, </color>. tag name is case sensitive
	/// unescape: @"\n" => "\n", @"\r" => "\r", @"\t" => "\t", for other char @"\?" => "?". '<' needs escape
	/// </summary>
	/// <param name="richText"></param>
	static IEnumerable<RichTextTagInfo> ExtractRichTextSpans(String richText)
	{
		Boolean isTagClose = false;
		ExtractRichTextTagState richState = ExtractRichTextTagState.TagBegin;
		Int32 lastNormalBegin = 0;
		Int32 tagBegin = 0;
		Int32 tagNameBegin = 0;
		Int32 tagParamBegin = 0;
		TagType tagType = TagType.None;

		Int32 iCur = 0;
		while (iCur < richText.Length)
		{
			Char ch = richText[iCur];
			switch (richState)
			{
				case ExtractRichTextTagState.Normal:
					if (ch == '<')
					{
						tagBegin = iCur;
						richState = ExtractRichTextTagState.TagBegin;
					}
					else if (ch == '\\')
					{
						richState = ExtractRichTextTagState.Escape;
						if (iCur != lastNormalBegin)
							yield return new RichTextTagInfo(new Substring(richText, lastNormalBegin, iCur-lastNormalBegin), TagType.None, false);
					}
					break;
				case ExtractRichTextTagState.Escape:
					String specialChar;
					switch (ch)
					{
						case 'n':
							specialChar = "\n";
							break;
						case 't':
							specialChar = "\t";
							break;
						default:
							specialChar = null;
							break;
					}
					if (specialChar == null)	//special char is just current char
						yield return new RichTextTagInfo(new Substring(richText, iCur, 1), TagType.None, false);
					else
						yield return new RichTextTagInfo(new Substring(specialChar), TagType.None, false);

					richState = ExtractRichTextTagState.Normal;
					lastNormalBegin = iCur + 1;
					break;
				case ExtractRichTextTagState.TagBegin:
					if (ch == '/')
					{
						isTagClose = true;
						tagNameBegin=iCur+1;
						richState = ExtractRichTextTagState.TagName;
					}
					else if (ch >= 'a' && ch <= 'z')
					{
						isTagClose = false;
						tagNameBegin=iCur;
						richState = ExtractRichTextTagState.TagName;
					}
					else
					{
						richState = ExtractRichTextTagState.Normal;
						continue;		//avoid skip current charater
					}
					break;
				case ExtractRichTextTagState.TagName:
					if (ch >= 'a' && ch <= 'z')
					{}
					else if (ch == '=')
					{

						if (isTagClose)		//tagClose can not has param
						{
							richState = ExtractRichTextTagState.Normal;
						}
						else
						{
							tagType = TagNameToTagType(new Substring(richText, tagNameBegin, iCur-tagNameBegin));
							if (tagType != TagType.None)
							{
								richState = ExtractRichTextTagState.TagParam;
								tagParamBegin = iCur+1;
							}
							else
							{
								richState = ExtractRichTextTagState.Normal;
							}
						}
					}
					else if (ch == '>')
					{
						richState = ExtractRichTextTagState.Normal;

						tagType = TagNameToTagType(new Substring(richText, tagNameBegin, iCur-tagNameBegin));
						if (tagType != TagType.None)
						{
							if (tagBegin != lastNormalBegin)
								yield return new RichTextTagInfo(new Substring(richText, lastNormalBegin, tagBegin-lastNormalBegin), TagType.None, false);
							yield return new RichTextTagInfo(new Substring(richText, tagBegin, iCur+1-tagBegin), tagType, isTagClose);
							lastNormalBegin = iCur+1;
						}
					}
					else
					{
						richState = ExtractRichTextTagState.Normal;
						continue;		//avoid skip current charater
					}
					break;
				case ExtractRichTextTagState.TagParam:
					if (ch == '>')
					{
						richState = ExtractRichTextTagState.Normal;

						if (tagBegin != lastNormalBegin)
							yield return new RichTextTagInfo(new Substring(richText, lastNormalBegin, tagBegin-lastNormalBegin), TagType.None, false);
						yield return new RichTextTagInfo(new Substring(richText, tagBegin, iCur+1-tagBegin), tagType, false, new Substring(richText, tagParamBegin, iCur-tagParamBegin));
						lastNormalBegin = iCur+1;
					}
					else if (ch == '<')
					{
						richState = ExtractRichTextTagState.Normal;
						continue;		//avoid skip current charater
					}
					else
					{
						//size param can only be digits
						if (tagType == TagType.Size)
						{
							if (!(ch >= '0' && ch <= '9'))
								richState = ExtractRichTextTagState.Normal;
						}
					}
					break;
				default:
					throw new Exception("wrong tagState");
			}

			++iCur;
		}
		if (iCur != lastNormalBegin)
			yield return new RichTextTagInfo(new Substring(richText, lastNormalBegin, iCur-lastNormalBegin), TagType.None, false);
	}

	struct TagStackItem
	{
		public Boolean isValid;
		public RichTextTagInfo tagInfo;

		public TagStackItem(RichTextTagInfo tagInfo) : this(true, tagInfo)
		{
		}

		TagStackItem(Boolean isValid, RichTextTagInfo tagInfo)
		{
			this.isValid = isValid;
			this.tagInfo = tagInfo;
		}
		public TagStackItem setAsInvalid()
		{
			return new TagStackItem(false, tagInfo);
		}
	}

	static void CalculateTextStyle(List<TagStackItem> tagList, List<Int32> affectiveTags, Boolean defaultBold, Boolean defaultItalic, out Boolean isBold, out Boolean isItalic, out Int32 size)
	{
		isBold = defaultBold;
		isItalic = defaultItalic;
		size = -1;
		for (Int32 i=0; i<affectiveTags.Count; ++i)
		{
			Int32 iTag = affectiveTags[i];
			var tagInfo = tagList[iTag].tagInfo;
			switch (tagInfo.tagType)
			{
				case TagType.Bold:
					isBold = true;
					break;
				case TagType.Italic:
					isItalic = true;
					break;
				case TagType.Size:
					size = 0;	//init
					if (tagInfo.tagParam.Length != 0)
					{
						if (!Int32.TryParse(tagInfo.tagParam.ToString(), out size))
							size = -1;
					}
					break;
				default:
					break;
			}
		}
	}

	public struct RichTextSpanInfo
	{
		public Substring text;
		public Boolean isVisible;
		public Boolean isBold;
		public Boolean isItalic;
		public Int32 size;

		public RichTextSpanInfo(Substring text, Boolean isVisible, Boolean isBold, Boolean isItalic, Int32 size)
		{
			this.text = text;
			this.isVisible = isVisible;
			this.isBold = isBold;
			this.isItalic = isItalic;
			this.size = size;
		}
	}

	static List<TagStackItem> s_tagList = new List<TagStackItem>();
	static Stack<Int32> s_tagStack = new Stack<Int32>();
	static List<Int32> s_affectiveTags = new List<Int32>();

	//onSpan(text, isVisible, isBold, isItalic, size)
	public static IEnumerable<RichTextSpanInfo> ParseRichText(String richText, Boolean defaultBold, Boolean defaultItalic)
	{
		List<TagStackItem> tagList = s_tagList;
		tagList.Clear();
		//first pass, extract tags
		foreach (var tagInfo in ExtractRichTextSpans(richText))
		{
			if (tagInfo.tagType != TagType.None)
			{
				tagList.Add(new TagStackItem(tagInfo));
			}
		}
	
		//mark invalid tags
		Stack<Int32> tagStack = s_tagStack;		//value is index in tagList
		tagStack.Clear();
		for (Int32 iTag=0; iTag<tagList.Count; ++iTag)
		{
			TagStackItem item = tagList[iTag];
			if (item.tagInfo.isClose)
			{
				if (tagStack.Count == 0)
				{
					tagList[iTag] = item.setAsInvalid();
				}
				else
				{
					Int32 lastOpenTagIndex = tagStack.Peek();
					if (item.tagInfo.tagType == tagList[lastOpenTagIndex].tagInfo.tagType)
					{
						tagStack.Pop();
					}
					else
					{
						tagList[iTag] = item.setAsInvalid();
					}
				}
			}
			else	//open
			{
				tagStack.Push(iTag);
			}
		}
		//the remaining tags are invalid
		while (tagStack.Count != 0)
		{
			Int32 iTag = tagStack.Pop();
			tagList[iTag] = tagList[iTag].setAsInvalid();
		}

		//second pass, output info
		Int32 tagIndex = 0;
		List<Int32> affectiveTags = s_affectiveTags;
		affectiveTags.Clear();

		Boolean isBold = false;
		Boolean isItalic = false;
		Int32 size = -1;

		foreach (var tagInfo in ExtractRichTextSpans(richText))
		{
			Substring spanContent = tagInfo.content;

			if (tagInfo.tagType != TagType.None)
			{
				if (tagList[tagIndex].isValid)
				{
					if (tagInfo.isClose)
						affectiveTags.Remove(affectiveTags[affectiveTags.Count-1]);
					else
						affectiveTags.Add(tagIndex);
					yield return new RichTextSpanInfo(spanContent, false, false, false, 0);

					CalculateTextStyle(tagList, affectiveTags, defaultBold, defaultItalic, out isBold, out isItalic, out size);
				}
				else	//treat as normal text
				{
					yield return new RichTextSpanInfo(spanContent, true, isBold, isItalic, size);
				}
				tagIndex++;
			}
			else
			{
				yield return new RichTextSpanInfo(spanContent, true, isBold, isItalic, size);
			}
		}
	}
}
