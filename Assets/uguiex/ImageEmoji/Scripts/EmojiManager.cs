﻿//#define EMOJI_RUNTIME
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EmojiUI
{
	public class EmojiManager : MonoBehaviour
	{
		private readonly List<EmojiSpriteAsset> _sharedAtlases = new List<EmojiSpriteAsset>();

		private readonly Dictionary<string, SpriteInfoGroup> _alltags = new Dictionary<string, SpriteInfoGroup>();

		private readonly List<string> tagIndexs = new List<string>();

		private readonly Dictionary<string, KeyValuePair<EmojiSpriteAsset, SpriteInfoGroup>> _spritemap = new Dictionary<string, KeyValuePair<EmojiSpriteAsset, SpriteInfoGroup>>();

		private IEmojiRender _render;

		[SerializeField]
		public List<EmojiSpriteAsset> PreparedAtlas = new List<EmojiSpriteAsset>();

		public bool HasInit { get; private set; }

#if UNITY_EDITOR
		[SerializeField]
		private bool _openDebug;
		protected bool OpenDebug
		{
			get
			{
				return _openDebug;
			}
			set
			{
				if (_openDebug != value)
				{
					_openDebug = value;
					if (Application.isPlaying)
					{
						if (value)
						{
							EmojiTools.StartDumpGUI();
						}
						else
						{
							EmojiTools.EndDumpGUI();
						}
					}
				}
			}
		}

		private List<EmojiSpriteAsset> _unityallAtlases;

		private List<string> _lostAssets;
#endif
		[SerializeField]
		private float _animationspeed = 5f;
		public float AnimationSpeed
		{
			get
			{
				return _animationspeed;
			}
			set
			{
				if (_render != null)
				{
					_render.Speed = value;
				}
				_animationspeed = value;
			}
		}

		[SerializeField]
		private EmojiRenderType _renderType = EmojiRenderType.RenderUnit;
		public EmojiRenderType RenderType
		{
			get
			{
				return _renderType;
			}
			set
			{
				if (_renderType != value)
				{
					_renderType = value;
					InitRender();
				}
			}
		}

		private static EmojiManager _instance = null;

		public static EmojiManager Instance
		{
			get{
				return _instance;
			}
		}

		public static int EmojiSize()
		{
			if (_instance == null)
				return 64;
			return (int)_instance._alltags[_instance.tagIndexs[0]].size;
		}

		public static int TotalCountOfEmoji()
		{
			if (_instance == null)
				return 0;
			return _instance._alltags.Count;
		}

		public static string EmojiKey(int index)
		{
			if (_instance == null)
				return "";

			if( index >= _instance.tagIndexs.Count )
				return "";    
			
			string tag = _instance.tagIndexs[index];
			if (tag == null )
				return string.Empty;
			return tag;
		}

		void Awake()
		{
			//Debug.Log("Emoji manager awake :" + gameObject.name + " " + gameObject.transform.parent.name);
			_instance = this;
#if UNITY_EDITOR
			if (OpenDebug)
			{
				EmojiTools.StartDumpGUI();
			}
#endif

			EmojiTools.BeginSample("Emoji_Init");
			Initialize();
			EmojiTools.EndSample();

			EmojiTools.AddUnityMemory(this);
		}

		void Initialize()
		{
			HasInit = true;
// #if UNITY_EDITOR
// 			string[] result = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(EmojiSpriteAsset).FullName));

// 			if (result.Length > 0 && _unityallAtlases == null)
// 			{
// 				_unityallAtlases = new List<EmojiSpriteAsset>(result.Length);
// 				for (int i = 0; i < result.Length; ++i)
// 				{
// 					string path = AssetDatabase.GUIDToAssetPath(result[i]);
// 					EmojiSpriteAsset asset = AssetDatabase.LoadAssetAtPath<EmojiSpriteAsset>(path);
// 					if (asset)
// 					{
// 						_unityallAtlases.Add(asset);
// 					}
// 				}
// 			}
// 			Debug.LogFormat("find :{0} atlas resource", result.Length);
// 			Debug.LogWarning("if your asset not in the resources please override InstantiateEmojiSpriteAsset");
// #endif


			EmojiTools.BeginSample("Emoji_Init");
			InitRender();
			EmojiTools.EndSample();

			EmojiTools.BeginSample("Emoji_preLoad");
			PreLoad();
			EmojiTools.EndSample();

			RebuildTagList();

			ForceRebuild();
		}

		void LateUpdate()
		{
			EmojiTools.BeginSample("Emoji_LateUpdate");
			if (_render != null)
			{
				_render.LateUpdate();
			}
			EmojiTools.EndSample();
		}

		private void OnDestroy()
		{
#if UNITY_EDITOR
			if (_lostAssets != null)
			{
				for (int i = 0; i < _lostAssets.Count; ++i)
				{
					string asset = _lostAssets[i];
					Debug.LogError(string.Format("not prepred atlasAsset named :{0}", asset));
				}
			}
#endif
			if (_render != null)
			{
				_render.Dispose();
			}

			_render = null;
			EmojiTools.RemoveUnityMemory(this);
		}

		protected virtual EmojiSpriteAsset InstantiateEmojiSpriteAsset(string filepath)
		{
			return Resources.Load<EmojiSpriteAsset>(filepath);
		}

		void InitRender()
		{
			if (_render == null || _render.renderType != RenderType)
			{

				if (RenderType == EmojiRenderType.RenderGroup)
				{
					EmojiRenderGroup newRender = new EmojiRenderGroup(this);
					newRender.Speed = AnimationSpeed;

					if (_render != null)
					{
						List<EmojiText> list = _render.GetAllRenders();
						if (list != null)
						{
							for (int i = 0; i < list.Count; ++i)
							{
								EmojiText text = list[i];
								if (text != null)
									newRender.TryRendering(text);
							}
						}

						List<EmojiSpriteAsset> atlaslist = _render.GetAllRenderAtlas();
						if (atlaslist != null)
						{
							for (int i = 0; i < atlaslist.Count; ++i)
							{
								EmojiSpriteAsset atlas = atlaslist[i];
								if (atlas != null)
									newRender.PrepareAtlas(atlas);
							}
						}
						_render.Dispose();
					}

					_render = newRender;

				}
				else if (RenderType == EmojiRenderType.RenderUnit)
				{
					UnitRender newRender = new UnitRender(this);
					newRender.Speed = AnimationSpeed;

					if (_render != null)
					{
						List<EmojiText> list = _render.GetAllRenders();
						if (list != null)
						{
							for (int i = 0; i < list.Count; ++i)
							{
								EmojiText text = list[i];
								if (text != null)
									newRender.TryRendering(text);
							}
						}

						List<EmojiSpriteAsset> atlaslist = _render.GetAllRenderAtlas();
						if (atlaslist != null)
						{
							for (int i = 0; i < atlaslist.Count; ++i)
							{
								EmojiSpriteAsset atlas = atlaslist[i];
								if (atlas != null)
									newRender.PrepareAtlas(atlas);
							}
						}

						_render.Dispose();
					}

					_render = newRender;
				}
				else
				{
					Debug.LogError("not support yet");
					this.enabled = false;
				}
			}
		}

		void PreLoad()
		{
			for (int i = 0; i < PreparedAtlas.Count; ++i)
			{
				EmojiSpriteAsset _spriteAsset = PreparedAtlas[i];
				PushRenderAtlas(_spriteAsset);
			}
		}

		void RebuildTagList()
		{
			EmojiTools.BeginSample("Emoji_rebuildTags");
			_alltags.Clear();
			_spritemap.Clear();
			tagIndexs.Clear();
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				EmojiSpriteAsset asset = _sharedAtlases[i];
				for (int j = 0; j < asset.listSpriteGroup.Count; ++j)
				{
					SpriteInfoGroup infogroup = asset.listSpriteGroup[j];
					SpriteInfoGroup group;
					if (_alltags.TryGetValue(infogroup.tag, out group))
					{
						Debug.LogErrorFormat("already exist :{0} ", infogroup.tag);
					}
					tagIndexs.Add(infogroup.tag);
					_alltags[infogroup.tag] = infogroup;
					_spritemap[infogroup.tag] = new KeyValuePair<EmojiSpriteAsset, SpriteInfoGroup>(asset, infogroup);
				}
				
			}
			EmojiTools.EndSample();
		}

		public IEmojiRender RegisterKey(EmojiText _key)
		{
			EmojiTools.BeginSample("Emoji_Register");
			if (_render != null)
			{
				if (_render.TryRendering(_key))
				{
					EmojiTools.EndSample();
					return _render;
				}
			}
			EmojiTools.EndSample();
			return null;
		}


		/// <summary>
		/// 移除文本 
		/// </summary>
		/// <param name="_id"></param>
		/// <param name="_key"></param>
		public void UnRegister(EmojiText _key)
		{
			EmojiTools.BeginSample("Emoji_UnRegister");
			if (_render != null)
			{
				_render.DisRendering(_key);
			}
			EmojiTools.EndSample();
		}

		public void ForceRebuild()
		{
			EmojiTools.BeginSample("Emoji_ForceRebuild");
			EmojiText[] alltexts = GetComponentsInChildren<EmojiText>();
			for (int i = 0; i < alltexts.Length; i++)
			{
				alltexts[i].SetVerticesDirty();
			}
			EmojiTools.EndSample();
		}

		/// <summary>
		/// 清除所有的精灵
		/// </summary>
		public void ClearAllSprites()
		{
			EmojiTools.BeginSample("Emoji_ClearAll");
			if (_render != null)
			{
				_render.Clear();
			}
			EmojiTools.EndSample();
		}

		public bool isRendering(EmojiSpriteAsset _spriteAsset)
		{
			return _spriteAsset != null && _render != null && _render.isRendingAtlas(_spriteAsset);
		}

		public bool CanRendering(string tagName)
		{
			return _alltags != null && _alltags.ContainsKey(tagName);
		}

		public bool CanRendering(int atlasId)
		{
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				EmojiSpriteAsset asset = _sharedAtlases[i];
				if (asset.ID == atlasId)
				{
					return true;
				}
			}
			return false;
		}

		public void PushRenderAtlas(EmojiSpriteAsset _spriteAsset)
		{
			EmojiTools.BeginSample("Emoji_PushRenderAtlas");
			if (!isRendering(_spriteAsset) && _spriteAsset != null)
			{
				_render.PrepareAtlas(_spriteAsset);

				if (!_sharedAtlases.Contains(_spriteAsset))
				{
					_sharedAtlases.Add(_spriteAsset);
				}
			}
			EmojiTools.EndSample();
		}

		public SpriteInfoGroup FindSpriteGroup(string TagName, out EmojiSpriteAsset resultatlas)
		{
			EmojiTools.BeginSample("Emoji_FindSpriteGroup");
			resultatlas = null;
			SpriteInfoGroup result = null;
			KeyValuePair<EmojiSpriteAsset, SpriteInfoGroup> data;
			if (_spritemap.TryGetValue(TagName,out data))
			{
				result = data.Value;
				resultatlas = data.Key;
			}
			EmojiTools.EndSample();
			return result;
		}

		public EmojiSpriteAsset FindAtlas(int atlasID)
		{
			EmojiTools.BeginSample("Emoji_FindAtlas");
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				EmojiSpriteAsset asset = _sharedAtlases[i];
				if (asset.ID.Equals(atlasID))
				{
					EmojiTools.EndSample();
					return asset;
				}
			}
			
			EmojiTools.EndSample();
			return null;
		}


		public EmojiSpriteAsset FindAtlas(string atlasname)
		{
			EmojiTools.BeginSample("FindAtlas");
			for (int i = 0; i < _sharedAtlases.Count; ++i)
			{
				EmojiSpriteAsset asset = _sharedAtlases[i];
				if (asset.AssetName.Equals(atlasname))
				{
					EmojiTools.EndSample();
					return asset;
				}
			}

			// EmojiSpriteAsset newasset = InstantiateEmojiSpriteAsset(atlasname);
			// if (newasset != null)
			// {
			// 	_sharedAtlases.Add(newasset);
			// }
			EmojiTools.EndSample();
			return null;

		}
	}
}


