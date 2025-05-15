using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class FrameAnimation : MonoBehaviour
{
	private Image ImageSource;
	private int mCurFrame = 0;
	private float mDelta = 0;
	private float mSecPerFrame  = 0.02f;

	public float Seconds = 3.0f;
	public int  Begin  = 1;
	public int  Step   = 1;
	public int  Count  = 1;
	public bool Foward = true;
	public bool AutoPlay = false;
	public bool Loop = false;
	public bool IsPlaying = false;
	public string SpritePath  = "abc.png";
	public string PrePath  = "Assets/arts/gui/atlas_v1/";
	public List<Sprite> SpriteFrames = new List<Sprite>();

	public int FrameCount
	{
		get
		{
			return SpriteFrames.Count;
		}
	}

	void Awake()
	{
		ImageSource = GetComponent<Image>();
		if (Count != 0) {
			mSecPerFrame = Seconds / Count;
		}
	}

	void Start()
	{
		if (AutoPlay)
		{
			Play();
		}
		else
		{
			IsPlaying = false;
		}
	}

	private void SetSprite(int idx)
	{
		ImageSource.sprite = SpriteFrames[idx];
		ImageSource.SetNativeSize();
	}

	public void Play()
	{
		IsPlaying = true;
		Foward = true;
	}

	public void PlayReverse()
	{
		IsPlaying = true;
		Foward = false;
	}

	void Update()
	{
		if (!IsPlaying || 0 == FrameCount)
		{
			return;
		}

		mDelta += Time.deltaTime;
		if (mDelta > mSecPerFrame)
		{
			mDelta = 0;
			if(Foward)
			{
				mCurFrame++;
			}
			else
			{
				mCurFrame--;
			}

			if (mCurFrame >= FrameCount)
			{
				if (Loop)
				{
					mCurFrame = 0;
				}
				else
				{
					IsPlaying = false;
					return;
				}
			}
			else if (mCurFrame<0)
			{
				if (Loop)
				{
					mCurFrame = FrameCount-1;
				}
				else
				{
					IsPlaying = false;
					return;
				}          
			}

			SetSprite(mCurFrame);
		}
	}

	public void Pause()
	{
		IsPlaying = false;
	}

	public void Resume()
	{
		if (!IsPlaying)
		{
			IsPlaying = true;
		}
	}

	public void Stop()
	{
		mCurFrame = 0;
		SetSprite(mCurFrame);
		IsPlaying = false;
	}

	public void Rewind()
	{
		mCurFrame = 0;
		SetSprite(mCurFrame);
		Play();
	}

	public void InitSprites(){
		#if UNITY_EDITOR
		SpriteFrames.Clear ();
		int num = Begin;
		for (int i = 0; i < Count; i++) {
			num = Begin + i * Step;
			string path = string.Format (PrePath + SpritePath, num);
			Sprite s = UnityEditor.AssetDatabase.LoadAssetAtPath (path, typeof(Sprite)) as Sprite;
			SpriteFrames.Add (s);
		}
		#endif
	}

}