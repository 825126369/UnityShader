using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseTextureCreater 
{
	public static int width = 256;
	public static int height = 256;

	public static Texture2D GetNoiseTexture(float scale)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                float sample = Mathf.PerlinNoise(i / (float)tex.width * scale, j / (float)tex.height * scale );
                tex.SetPixel(i, j, new Color(sample, sample, sample, sample));
            }
        }
        tex.Apply();
		
        return tex;
    }

	public static Texture2D GetNoiseTextureByScaleAdd(float fCoef, float scale)
	{
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                float sample1 = Mathf.PerlinNoise(i / (float)tex.width * fCoef, j / (float)tex.height * fCoef);
				float sample = sample1 * (1 + scale);
                tex.SetPixel(i, j, new Color(sample, sample, sample, sample));
            }
        }
        tex.Apply();
		
        return tex;

	}

	public static Texture2D GetNoiseTextureByScaleAdd1(params float[] scale)
	{
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
				float sample = 0.0f;
				for (int k = 0; k < scale.Length; k ++)
				{
					float sample1 = Mathf.PerlinNoise(i / (float)tex.width * scale[k], j / (float)tex.height * scale[k]);
					sample += sample1;
				}
				sample = sample / scale.Length;
                tex.SetPixel(i, j, new Color(sample, sample, sample, 0));
            }
        }
        tex.Apply();
		
        return tex;

	}

}
