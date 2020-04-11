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

	public static Texture2D GetNoiseTextureByScaleAdd(params float[] scale)
	{
		Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Mirror;

        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
				float sample = 0.0f;
				for (int k = 0; k < scale.Length; k ++)
				{
					float sample1 = Mathf.PerlinNoise(i / (float)tex.width * scale[k], j / (float)tex.height * scale[k]);
                    sample1 = Mathf.Clamp01(sample1);
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
