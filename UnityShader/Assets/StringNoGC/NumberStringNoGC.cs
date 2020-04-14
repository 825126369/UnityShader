using System;
using System.Text;
using UnityEngine;

public static class NumberStringNoGC
{
    private static readonly char[] ms_digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
  
    public static StringBuilder AppendUInt64(this StringBuilder string_builder, UInt64 UInt64_val)
    {
        int offset = string_builder.Length;
        int length = 0;
        UInt64 length_calc = UInt64_val;

        do
        {
            length_calc /= 10;
            length++;
        }
        while (length_calc > 0);

        string_builder.Length = offset + length;

        int nLastIndex = string_builder.Length - 1;

        while (nLastIndex >= offset)
        {
            string_builder[nLastIndex] = ms_digits[UInt64_val % 10];
            UInt64_val /= 10;
            nLastIndex--;
        }
        
        return string_builder;
    }

    public static StringBuilder Align(this StringBuilder string_builder, TextAlignment align)
    {
        string_builder.Length = string_builder.Capacity;

        int nLeftSpaceCount = 0;
        for(int i = 0; i < string_builder.Capacity; i++)
        {
            if(string_builder[i] == ' ')
            {
                nLeftSpaceCount = nLeftSpaceCount + 1;
            }
            else
            {
                break;
            }
        }

        int nRightSpaceCount = 0;
        for (int i = string_builder.Capacity - 1; i >= 0; i--)
        {
            if (string_builder[i] == ' ')
            {
                nRightSpaceCount = nRightSpaceCount + 1;
            }
            else
            {
                break;
            }
        }

        if (align == TextAlignment.Center)
        {
            int offset = (nLeftSpaceCount - nRightSpaceCount) / 2;
            if (offset > 0)
            {
                for (int i = nLeftSpaceCount; i < string_builder.Capacity - nRightSpaceCount; i++)
                {
                    string_builder[i - offset] = string_builder[i];
                }

                for (int i = 0; i < offset; i++)
                {
                    string_builder[string_builder.Capacity - 1 - nRightSpaceCount - i] = ' ';
                }
            }
            else
            {
                for (int i = string_builder.Capacity - nRightSpaceCount - 1; i >= nLeftSpaceCount; i--)
                {
                    string_builder[i + offset] = string_builder[i];
                }

                for (int i = 0; i < offset; i++)
                {
                    string_builder[nLeftSpaceCount + i] = ' ';
                }
            }
        }
        else if (align == TextAlignment.Right)
        {
            int offset = nRightSpaceCount;
            if (offset > 0)
            {
                for (int i = 0; i < nRightSpaceCount; i++)
                {
                    char iPosChar = string_builder[string_builder.Capacity - 1 - i];
                    string_builder[string_builder.Capacity - 1 - i] = string_builder[string_builder.Capacity - 1 - i - nRightSpaceCount];
                    string_builder[string_builder.Capacity - 1 - i - nRightSpaceCount] = iPosChar;
                }
            }
        }
        else if (align == TextAlignment.Left)
        {
            int offset = nLeftSpaceCount;
            if (offset > 0)
            {
                for (int i = 0; i < nLeftSpaceCount; i++)
                {
                    char iPosChar = string_builder[i];
                    string_builder[i] = string_builder[i + nLeftSpaceCount];
                    string_builder[i + nLeftSpaceCount] = iPosChar;
                }
            }
        }

        return string_builder;
    }

    public static StringBuilder Align(this StringBuilder string_builder, TextAnchor align)
    {
        if (align == TextAnchor.LowerLeft || align == TextAnchor.MiddleLeft || align == TextAnchor.UpperLeft)
        {
            return Align(string_builder, TextAlignment.Left);
        }
        else if (align == TextAnchor.LowerCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.UpperCenter)
        {
            return Align(string_builder, TextAlignment.Center);
        }
        else
        {
            return Align(string_builder, TextAlignment.Right);
        }
    }


    public static string GetGarbageFreeString(this StringBuilder string_builder)
    {
        return (string)string_builder.GetType().GetField("_str", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(string_builder);
    }

    public static void GarbageFreeClear(this StringBuilder string_builder)
    {
        string_builder.Length = 0;
        string_builder.Append(' ', string_builder.Capacity);
        string_builder.Length = 0;
    }
}