/// ========================================================
/// file：InlineText.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// 修改：在原来的基础上支持超链接功能 by givens
/// 表情动画最多支持8帧
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

    //[ExecuteInEditMode]
public class InlineText : Text, IPointerClickHandler
{
    /// <summary>
    /// 用正则取  [ID1#ID2#ID3#ID4#Content]
    /// 1,ID1: 标示超链接类型
    /// 2,ID2: 扩展位
    /// 3,ID3: 扩展位
    /// 4,ID4: 扩展位
    /// 5,ID5: 扩展位     -?\d{0,}_?-?\d{0,}
    /// 6,Content:如果是超链接，则标示显示的内容 
    /// </summary>
    private static readonly Regex _InputTagRegex = new Regex(@"\[(-{0,1}\d{0,})#(-{0,1}\d{0,})#(-{0,1}\d{0,})#(-{0,1}\d{0,})#(.+?)#(.+?)\]", RegexOptions.Singleline);

    /// <summary>
    /// 聊天表情的静态信息
    /// </summary>
    private static Dictionary<string, EmojiInfo> EmojiIndex = null;
    private static Dictionary<int, string> EmojiKeys = null;
    private static string emojiConfig = "";
    /// <summary>
    /// 当前带渲染的表情信息
    /// </summary>
    Dictionary<int, EmojiInfo> emojiDic = new Dictionary<int, EmojiInfo>();
    /// <summary>
    /// 保存自定义表情信息
    /// </summary>
    private Dictionary<string, EmojiInfo> overrideEmojiInfos = null;
    private Dictionary<string, string> overrideNameDict = null;
    /// <summary>
    /// 默认表情配置
    /// </summary>
    [HideInInspector]
    public TextAsset emojiContent = null;
    /// <summary>
    /// 自定义表情设置
    /// </summary>
   [Tooltip("自定义表情设置,同时要更新材质")] 
    public TextAsset overrideEmojiContent = null;

    /// <summary>
    /// 保存字符串，防止TextAsset被GC掉
    /// </summary>
    private string overrideConfig = "";

    [Tooltip("表情缩放")]
    public float emojiScale = 6f;
    [Tooltip("表情偏移")]
    public Vector2 emojiOffset = new Vector2(-2f, 2f);

    struct EmojiInfo
    {
        public float x;
        public float y;
        public float size;
        public int len;
    }

    readonly UIVertex[] m_TempVerts = new UIVertex[4];

    //private InlineManager _InlineManager = null;

    //更新后的文本
    private string _OutputText = "";

    #region 超链接回调
    [System.Serializable]
    public class HrefClickEvent : UnityEvent<string> { }
    //点击事件监听
    public HrefClickEvent OnHrefClick = new HrefClickEvent();
    // 超链接信息列表  
    private readonly List<HrefInfo> _ListHrefInfos = new List<HrefInfo>();
    #endregion

    protected override void Awake()
    {
        if (InlineText.emojiConfig.Length == 0 && emojiContent != null)
        {
            InlineText.emojiConfig = emojiContent.text;
        }
        if (overrideEmojiContent != null)
        {
            overrideConfig = overrideEmojiContent.text;
            InitOverrideEmojiInfo();
        }
        ActiveText();
    }

    public static int TotalCountOfEmoji()
    {
        if (EmojiIndex == null)
        {
            InitEmojiIndex();
        }
        return EmojiIndex.Count;
    }

    public static string EmojiKey(int index)
    {
        if (EmojiKeys == null)
        {
            InitEmojiIndex();
        }
        string key = string.Empty;
        if (EmojiKeys.TryGetValue(index, out key))
        {
            return key;
        }

        return string.Empty;
    }

    private static void InitEmojiIndex()
    {
        EmojiIndex = new Dictionary<string, EmojiInfo>();
        EmojiKeys = new Dictionary<int, string>();

        if (emojiConfig.Length == 0) return;

        //load emoji data, and you can overwrite this segment code base on your project.
        //TextAsset emojiContent = Resources.Load<TextAsset>("emoji");
        string[] lines = InlineText.emojiConfig.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                string[] strs = lines[i].Split('\t');
                EmojiInfo info;
                info.x = float.Parse(strs[3]);
                info.y = float.Parse(strs[4]);
                info.size = float.Parse(strs[5]);
                info.len = 0;
                EmojiIndex.Add(strs[1], info);
                EmojiKeys.Add(i, strs[1]);
            }
        }
    }

    private void InitOverrideEmojiInfo()
    {
        overrideEmojiInfos = new Dictionary<string, EmojiInfo>();
        overrideNameDict = new Dictionary<string, string>();
        if (overrideConfig.Length == 0) return;

        string[] lines = overrideConfig.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(lines[i]))
            {
                string[] strs = lines[i].Split('\t');
                EmojiInfo info;
                info.x = float.Parse(strs[3]);
                info.y = float.Parse(strs[4]);
                info.size = float.Parse(strs[5]);
                info.len = 0;
                string key = strs[1];
                overrideEmojiInfos.Add(key, info);
                
                string name = strs[0];
                name = name.Replace("{", "");
                name = name.Replace("}", "");
                //Debug.Log("override info :" + name + " " + key);
                overrideNameDict.Add(name, key);
            }
        }
    }

    public string GetOverrideEmojikey(string name )
    {
        if (overrideNameDict != null)
        {
            return overrideNameDict[name];
        }
        return "[0]";
    }

    public void ActiveText()
    {
        //支持富文本
        supportRichText = true;
        //对齐几何
        alignByGeometry = false;
        //启动的是 更新顶点
        SetVerticesDirty();
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        //设置新文本
        _OutputText = GetOutputText();
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        if (InlineText.emojiConfig.Length == 0 && emojiContent != null)
        {
            InlineText.emojiConfig = emojiContent.text;
        }

        if (overrideEmojiContent != null && overrideConfig.Length == 0)
        {
            overrideConfig = overrideEmojiContent.text;
            InitOverrideEmojiInfo();
        }

        if (EmojiIndex == null)
        {
            if (EmojiIndex == null)
            {
                InitEmojiIndex();
            }
        }

        emojiDic.Clear();
        if (supportRichText)
        {
            var emojiDict = overrideConfig.Length > 0 ? overrideEmojiInfos : InlineText.EmojiIndex;
            MatchCollection matches = Regex.Matches(_OutputText, "\\[[a-z0-9A-Z]+\\]");
            for (int i = 0; i < matches.Count; i++)
            {
                EmojiInfo info;
                if (emojiDict.TryGetValue(matches[i].Value, out info))
                {
                    info.len = matches[i].Length;
                    emojiDic.Add(matches[i].Index, info);
                }
            }
        }
        //string populateStr = "";
        //Regex r = new Regex(@"\[[a-z0-9A-Z]+\]", RegexOptions.Singleline);
        //if (r.IsMatch(_OutputText))
        //{
        //    populateStr = r.Replace(_OutputText, "@");
        //    Debug.Log(populateStr);
        //}

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate(_OutputText, settings);

        Rect inputRect = rectTransform.rect;

        // get the text alignment anchor point for the text in local space
        Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        Vector2 refPoint = Vector2.zero;
        refPoint.x = Mathf.Lerp(inputRect.xMin, inputRect.xMax, textAnchorPivot.x);
        refPoint.y = Mathf.Lerp(inputRect.yMin, inputRect.yMax, textAnchorPivot.y);

        // Determine fraction of pixel to offset text mesh.
        Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line...
        int vertCount = verts.Count - 4;

        toFill.Clear();

        CalcBoundsInfo(verts, toFill, settings);

        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            float repairDistance = 0;
            float repairDistanceHalf = 0;
            float repairY = 0;
            if (vertCount > 0)
            {
                repairY = verts[3].position.y;
            }
            for (int i = 0; i < vertCount; i++)
            {


                EmojiInfo info;
                int index = i / 4;
                if (emojiDic.TryGetValue(index, out info))
                {
                   

                    //compute the distance of '[' and get the distance of emoji 
                    float charDis = (verts[i + 1].position.x - verts[i].position.x) * 3;
                    m_TempVerts[3] = verts[i];//1
                    m_TempVerts[2] = verts[i + 1];//2
                    m_TempVerts[1] = verts[i + 2];//3
                    m_TempVerts[0] = verts[i + 3];//4

                    //Debug.Log("===================START==================" + index);
                    //Debug.Log("add vertex ori :" + m_TempVerts[0].position);
                    //Debug.Log("m_TempVerts[0] 1 :" + m_TempVerts[0].position);
                    //the real distance of an emoji
                    m_TempVerts[2].position += new Vector3(charDis, 0, 0);
                    m_TempVerts[1].position += new Vector3(charDis, 0, 0);

                    //make emoji has equal width and height
                    float fixValue = (m_TempVerts[2].position.x - m_TempVerts[3].position.x - (m_TempVerts[2].position.y - m_TempVerts[1].position.y));
                    m_TempVerts[2].position -= new Vector3(fixValue, 0, 0);
                    m_TempVerts[1].position -= new Vector3(fixValue, 0, 0);

                    float curRepairDis = 0;
                    if (verts[i + 2].position.y < repairY)
                    {// to judge current char in the same line or not
                        repairDistance = repairDistanceHalf;
                        repairDistanceHalf = 0;
                        repairY = verts[i + 3].position.y;

                        //Debug.Log("judge same line :" + index + " " + repairDistance + " " + repairY);
                    }

                    curRepairDis = repairDistance;
                    int dot = 0;//repair next line distance
                    for (int j = info.len - 1; j > 0; j--)
                    {
                        if (verts[i + j * 4 + 3].position.y >= verts[i + 3].position.y)
                        {
                            repairDistance += verts[i + j * 4 + 1].position.x - m_TempVerts[2].position.x;
                            break;
                        }
                        else
                        {
                            dot = i + 4 * j;

                        }
                    }
                    if (dot > 0)
                    {
                        int nextChar = i + info.len * 4;
                        if (nextChar < verts.Count)
                        {
                            repairDistanceHalf = verts[nextChar].position.x - verts[dot].position.x;
                        }
                    }

                    //Debug.Log("curRepairDis :" + curRepairDis);
                    //repair its distance
                    for (int j = 0; j < 4; j++)
                    {
                        m_TempVerts[j].position -= new Vector3(curRepairDis, 0, 0);
                    }

                    //for (int vextIndex = 0; vextIndex < 4; vextIndex++)
                    //{

                    //}
                    //Debug.Log(" vertex res :" + m_TempVerts[0].position + " " + charDis);
                    //Debug.Log("offsets :" + " " + curRepairDis + " " + repairDistance + " " + repairDistanceHalf);

                    //Debug.Log("===================END====================" + index);

                    m_TempVerts[0].position *= unitsPerPixel;
                    m_TempVerts[1].position *= unitsPerPixel;
                    m_TempVerts[2].position *= unitsPerPixel;
                    m_TempVerts[3].position *= unitsPerPixel;


                    m_TempVerts[0].position += new Vector3(-emojiScale + emojiOffset.x, -emojiScale + emojiOffset.y, 0);
                    m_TempVerts[3].position += new Vector3(-emojiScale + emojiOffset.x, emojiScale + emojiOffset.y, 0);
                    m_TempVerts[1].position += new Vector3(emojiScale + emojiOffset.x, -emojiScale + emojiOffset.y, 0);
                    m_TempVerts[2].position += new Vector3(emojiScale + emojiOffset.x, emojiScale + emojiOffset.y, 0);


                    //Debug.Log("m_TempVerts[0] 1 :" + m_TempVerts[0].position);
                    //Debug.Log("m_TempVerts[0] 2 :" + m_TempVerts[1].position);
                    //Debug.Log("m_TempVerts[0] 3 :" + m_TempVerts[2].position);
                    //Debug.Log("m_TempVerts[0] 4 :" + m_TempVerts[3].position);

                    float pixelOffset = emojiDic[index].size / 32 / 2;
                    m_TempVerts[0].uv1 = new Vector2(emojiDic[index].x + pixelOffset, emojiDic[index].y + pixelOffset);
                    m_TempVerts[1].uv1 = new Vector2(emojiDic[index].x - pixelOffset + emojiDic[index].size, emojiDic[index].y + pixelOffset);
                    m_TempVerts[2].uv1 = new Vector2(emojiDic[index].x - pixelOffset + emojiDic[index].size, emojiDic[index].y - pixelOffset + emojiDic[index].size);
                    m_TempVerts[3].uv1 = new Vector2(emojiDic[index].x + pixelOffset, emojiDic[index].y - pixelOffset + emojiDic[index].size);

                    toFill.AddUIVertexQuad(m_TempVerts);

                    i += 4 * info.len - 1;
                }
                else
                {
                    int tempVertsIndex = i & 3;
                    if (tempVertsIndex == 0 && verts[i + 2].position.y < repairY)
                    {
                        repairY = verts[i + 3].position.y;
                        repairDistance = repairDistanceHalf;
                        repairDistanceHalf = 0;
                    }
                    m_TempVerts[tempVertsIndex] = verts[i];
                    m_TempVerts[tempVertsIndex].position -= new Vector3(repairDistance, 0, 0);
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }
            }

        }
        m_DisableFontTextureRebuiltCallback = false;
    }

    #region 文本所占的长宽
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(_OutputText, settings) / pixelsPerUnit;
        }
    }
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(_OutputText, settings) / pixelsPerUnit;
        }
    }
    #endregion


    #region 处理超链接的包围盒
    void CalcBoundsInfo(IList<UIVertex> verts, VertexHelper toFill, TextGenerationSettings settings)
    {
        #region 包围框
        // 处理超链接包围框  
        UIVertex vert = new UIVertex();
        foreach (var hrefInfo in _ListHrefInfos)
        {
            hrefInfo.boxes.Clear();
            if (hrefInfo.startIndex >= verts.Count)
            {
                continue;
            }
            // 将超链接里面的文本顶点索引坐标加入到包围框  
            vert = verts[hrefInfo.startIndex];
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= verts.Count)
                {
                    break;
                }

                vert = verts[i];
                pos = vert.position;
                //Debug.LogWarningFormat("pos = {0}", pos);

                if (pos.x < bounds.min.x)
                {
                    // 换行重新添加包围框  
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框  
                }

                //if (i % 4 == 3)
                //{
                //    Debug.LogWarningFormat("正在处理文字 {0}", _OutputText[i / 4]);
                //    Debug.LogWarningFormat("min = {0}, size = {1}", bounds.min, bounds.size);
                //}

            }
            //添加包围盒
            hrefInfo.boxes.Add(new Rect(bounds.min / pixelsPerUnit, bounds.size / pixelsPerUnit));
        }
        #endregion

        /*
        #region 添加下划线
        TextGenerator _UnderlineText = new TextGenerator();
        _UnderlineText.Populate("_", settings);
        IList<UIVertex> _TUT = _UnderlineText.verts;
        foreach (var item in _ListHrefInfos)
        {
            for (int i = 0; i < item.boxes.Count; i++)
            {
                //计算下划线的位置
                Vector3[] _ulPos = new Vector3[4];
                _ulPos[0] = item.boxes[i].position + new Vector2(0.0f, fontSize * 0.2f);
                _ulPos[1] = _ulPos[0]+new Vector3(item.boxes[i].width,0.0f);
                _ulPos[2] = item.boxes[i].position + new Vector2(item.boxes[i].width, 0.0f);
                _ulPos[3] =item.boxes[i].position;
                //绘制下划线
                for (int j = 0; j < 4; j++)
                {
                    m_TempVerts[j] = _TUT[j];
                    m_TempVerts[j].color = Color.blue;
                    m_TempVerts[j].position = _ulPos[j];
                    if (j == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }

            }
        }

        #endregion
        */
    }
    #endregion

    //public string UpdateHrefColor(string hrefText,string color16)
    //{
    //    Match match = _InputTagRegex.Match(hrefText);

    //}

    #region 根据正则规则更新文本
    private string GetOutputText()
    {
        _ListHrefInfos.Clear();
        StringBuilder _textBuilder = new StringBuilder();
        int _textIndex = 0;

        MatchCollection collection = _InputTagRegex.Matches(text);
        foreach (Match match in collection)
        {
            int _tempID1 = 0;
            uint _tempID2 = 0;
            uint _tempID3 = 0;
            int _tempID4 = 0;
            string _tempID5 = "";
            if (!string.IsNullOrEmpty(match.Groups[1].Value))
                _tempID1 = int.Parse(match.Groups[1].Value);
            if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                _tempID2 = uint.Parse(match.Groups[2].Value);
            }
            if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                _tempID3 = uint.Parse(match.Groups[3].Value);
            }
            if (!string.IsNullOrEmpty(match.Groups[4].Value))
            {
                _tempID4 = int.Parse(match.Groups[4].Value);
            }
            if (!string.IsNullOrEmpty(match.Groups[5].Value))
            {
                _tempID5 = match.Groups[5].Value;
            }
            string _tempTag = match.Groups[6].Value;
            //更新超链接
            if (_tempID1 >= 0)
            {
                _textBuilder.Append(text.Substring(_textIndex, match.Index - _textIndex));
                // _textBuilder.Append("<color=blue>");
                int _startIndex = _textBuilder.Length * 4;
                if (_tempID1 != 8)
                {
                    _textBuilder.Append("[");
                }
                _textBuilder.Append(_tempTag);
                if (_tempID1 != 8)
                {
                    _textBuilder.Append("]");
                }
                int _endIndex = _textBuilder.Length * 4; //- 2;
                //_textBuilder.Append("</color>");

                var hrefInfo = new HrefInfo
                {
                    ID1 = _tempID1,
                    ID2 = _tempID2,
                    ID3 = _tempID3,
                    ID4 = _tempID4,
                    ID5 = _tempID5,
                    startIndex = _startIndex, // 超链接里的文本起始顶点索引
                    endIndex = _endIndex,
                    name = _tempTag
                };
                _ListHrefInfos.Add(hrefInfo);

            }
            _textIndex = match.Index + match.Length;
        }

        _textBuilder.Append(text.Substring(_textIndex, text.Length - _textIndex));
        return _textBuilder.ToString();
    }
    #endregion

    #region  超链接信息类
    private class HrefInfo
    {
        public int ID1;
        public uint ID2;
        public uint ID3;
        public int ID4;
        public string ID5;

        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }
    #endregion




    #region 点击事件检测是否点击到超链接文本

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        //Debug.LogFormat("point pos{0}", lp);
        foreach (var hrefInfo in _ListHrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                //Debug.LogWarningFormat("min = {0} size = {1} pos = {2}", boxes[i].min, boxes[i].max, boxes[i].position);
                if (boxes[i].Contains(lp))
                {
                    Debug.LogWarning("---click hit----");
                    OnHrefClick.Invoke(hrefInfo.name);
                    //lua callback
                    return;
                }
            }
        }
    }
    #endregion

}
