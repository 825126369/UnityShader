using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEngine.TextCore;
using System.Reflection;

namespace TMPro
{

    public class TextMeshProFontEditor : Editor
    {
        static List<Glyph> m_FontGlyphTable = new List<Glyph>();
        static List<TMP_Character> m_FontCharacterTable = new List<TMP_Character>();

        [MenuItem("Tools/Create Font")]
        public static void CreateFont()
        {
            Font mFont = Selection.activeObject as Font;
            if (mFont)
            {
                string filePath = AssetDatabase.GetAssetPath(mFont);
                string tex_DirName = Path.GetDirectoryName(filePath);
                string tex_FileName = Path.GetFileNameWithoutExtension(filePath);
                string tex_Path_NoExt = tex_DirName + "/" + tex_FileName;

                TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(tex_Path_NoExt + ".asset");
                if (fontAsset)
                {
                    DestroyImmediate(fontAsset, true);
                }

                TMP_FontAsset mFontAsset = TMP_FontAsset.CreateFontAsset(mFont);
                AssetDatabase.CreateAsset(mFontAsset, tex_Path_NoExt + ".asset");

                Texture2D oriTex = mFont.material.mainTexture as Texture2D;
                Texture2D m_FontAtlasTexture = new Texture2D(oriTex.width, oriTex.height, oriTex.format, false);
                for(int i = 0; i < m_FontAtlasTexture.width; i++)
                {
                    for (int j = 0; j < m_FontAtlasTexture.height; j++)
                    {
                        m_FontAtlasTexture.SetPixel(i, j, oriTex.GetPixel(i, j));
                    }
                }

                m_FontAtlasTexture.Apply();

                m_FontAtlasTexture.name = tex_FileName + " Atlas";
                mFontAsset.GetType().GetField("m_AtlasWidth", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mFontAsset, m_FontAtlasTexture.width);
                mFontAsset.GetType().GetField("m_AtlasHeight", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mFontAsset, m_FontAtlasTexture.height);
                mFontAsset.GetType().GetField("m_AtlasPadding", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mFontAsset, 5);
                mFontAsset.atlasTextures = new Texture2D[] { (Texture2D)m_FontAtlasTexture };

                Shader default_Shader = Shader.Find("TextMeshPro/Bitmap"); // m_shaderSelection;
                Material tmp_material = new Material(default_Shader);
                tmp_material.name = tex_FileName + " Material";
                tmp_material.SetTexture(ShaderUtilities.ID_MainTex, m_FontAtlasTexture);
                mFontAsset.material = tmp_material;

                foreach (var v in mFont.characterInfo)
                {
                    uint unicode = (uint)v.index;
                    Glyph mGlyph = GetGlyph(v);

                    m_FontGlyphTable.Add(mGlyph);
                    m_FontCharacterTable.Add(new TMP_Character(unicode, mGlyph));
                }
        
                mFontAsset.GetType().GetField("m_CharacterTable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mFontAsset, m_FontCharacterTable);
                mFontAsset.GetType().GetField("m_GlyphTable", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mFontAsset, m_FontGlyphTable);

                FaceInfo mFaceInfo = GetFaceInfo(mFont);
                mFontAsset.GetType().GetField("m_FaceInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(mFontAsset, mFaceInfo);

                m_FontAtlasTexture.hideFlags = HideFlags.None;
                mFontAsset.material.hideFlags = HideFlags.None;
                
                AssetDatabase.AddObjectToAsset(m_FontAtlasTexture, mFontAsset);
                AssetDatabase.AddObjectToAsset(tmp_material, mFontAsset);

                AssetDatabase.SaveAssets();

                mFontAsset.ReadFontAssetDefinition();

                AssetDatabase.Refresh();
            }

        }

        private static Glyph GetGlyph(CharacterInfo mInfo)
        {
            Glyph mGlyph = new Glyph();
            mGlyph.index = (uint)mInfo.index;
            mGlyph.scale = 1;
            mGlyph.glyphRect = new GlyphRect(Rect.MinMaxRect(mInfo.minX, mInfo.minY, mInfo.maxX, mInfo.maxY));
            mGlyph.atlasIndex = 0;

            GlyphMetrics mGlyphMetrics = new GlyphMetrics(mInfo.glyphWidth, mInfo.glyphHeight, mInfo.bearing, 0, mInfo.advance);
            mGlyph.metrics = mGlyphMetrics;

            return mGlyph;
        }

        private static FaceInfo GetFaceInfo(Font mFont)
        {
            int minPointSize = 0;
            int maxPointSize = (int)Mathf.Sqrt((mFont.material.mainTexture.width * mFont.material.mainTexture.height) / mFont.characterInfo.Length) * 3;
            int m_PointSize = (maxPointSize + minPointSize) / 2;

            FaceInfo mFaceInfo = new FaceInfo();
            mFaceInfo.familyName = mFont.name;
            mFaceInfo.styleName = "Regular";
            mFaceInfo.scale = 1;
            mFaceInfo.pointSize = m_PointSize;
            mFaceInfo.lineHeight = mFont.lineHeight;
            mFaceInfo.ascentLine = mFont.ascent;
            mFaceInfo.capLine = 0;
            mFaceInfo.meanLine = 0;
            mFaceInfo.baseline = 0;
            return mFaceInfo;
        }

    }
}
