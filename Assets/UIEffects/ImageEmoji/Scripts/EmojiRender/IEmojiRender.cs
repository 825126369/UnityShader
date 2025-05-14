using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmojiUI
{
	public enum EmojiRenderType
	{
		RenderGroup,
		RenderUnit,
		RenderBoth,
	}

	public interface IEmojiRender
	{
		EmojiRenderType renderType { get; }

		float Speed { get; set; }

		List<EmojiText> GetAllRenders();

		List<EmojiSpriteAsset> GetAllRenderAtlas();

		Texture getRenderTexture(EmojiSpriteGraphic graphic);

		bool isRendingAtlas(EmojiSpriteAsset asset);

		void PrepareAtlas(EmojiSpriteAsset asset);

		bool TryRendering(EmojiText text);

		void DisRendering(EmojiText text);

		void Clear();

		void Release(Graphic graphic);

		void FillMesh(Graphic graphic, VertexHelper vh);

		void LateUpdate();

		void DrawGizmos(Graphic graphic);

		void Dispose();
	}
}


