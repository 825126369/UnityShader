using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseTextureCreater 
{
    public class NoiseInfo
    {
        public float fWeight = 0.5f;
        public float fFrequency = 0.5f;
    }

	public static int width = 256;
	public static int height = 256;

	public static Texture2D GetNoiseTexture(float scale)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Mirror;
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                float sample = Mathf.PerlinNoise(i / (float)tex.width * scale, j / (float)tex.height * scale );
                sample = Mathf.Clamp01(sample);
                tex.SetPixel(i, j, new Color(sample, sample, sample, sample));
            }
        }
        tex.Apply();
		
        return tex;
    }

	public static Texture2D GetNoiseTextureByScaleAdd(params NoiseInfo[] NoiseInfoList)
	{
        float[] mWeightArray = new float[NoiseInfoList.Length];

        float fSumWeight = 0.0f;
        for(int i = 0; i < NoiseInfoList.Length; i++)
        {
           fSumWeight += NoiseInfoList[i].fWeight;
        }

        for(int i = 0; i < NoiseInfoList.Length; i++)
        {
           mWeightArray[i] = NoiseInfoList[i].fWeight / fSumWeight;
        }

		Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Mirror;

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
				float sample = 0.0f;
				for (int k = 0; k < NoiseInfoList.Length; k ++)
				{
					float sample1 = Mathf.PerlinNoise(i / (float)tex.width * NoiseInfoList[k].fFrequency, j / (float)tex.height * NoiseInfoList[k].fFrequency);
                    sample1 = Mathf.Clamp01(sample1);
					sample += sample1 * mWeightArray[k];
				}
                
                tex.SetPixel(i, j, new Color(sample, sample, sample, sample));
            }
        }
        tex.Apply();
		
        return tex;
	}

}
