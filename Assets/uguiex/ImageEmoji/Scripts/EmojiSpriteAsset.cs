using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using EmojiUI;

namespace EmojiUI
{
	[SerializeField]
	public class EmojiSpriteAsset : ScriptableObject
	{

		public string AssetName;
		/// <summary>
		/// 图集ID
		/// </summary>
		public int ID;
		/// <summary>
		/// 图片资源
		/// </summary>
		public Texture texSource;
		/// <summary>
		/// 所有sprite信息 EmojiSpriteAssetInfor类为具体的信息类
		/// </summary>
		public List<SpriteInfoGroup> listSpriteGroup = new List<SpriteInfoGroup>();

	}
}



