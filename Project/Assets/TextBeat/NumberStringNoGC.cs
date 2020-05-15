using System;
using System.Text;
using UnityEngine;

namespace TextBeat
{
    public static class NumberStringNoGC
    {
        private static readonly char[] ms_digits = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private static char Placeholder = '^';

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

        public static StringBuilder AppendUInt64WithCommas(this StringBuilder string_builder, UInt64 UInt64_val)
        {
            int offset = string_builder.Length;
            int length = 0;
            int numCommas = 0;
            UInt64 length_calc = UInt64_val;

            do
            {
                length_calc /= 10;
                length++;

                if (length % 3 == 0)
                {
                    numCommas++;
                }
            }
            while (length_calc > 0);

            if(length % 3 == 0)
            {
                numCommas--;
            }

            string_builder.Length = offset + length + numCommas;

            int nLastIndex = string_builder.Length - 1;
            int nCommasIndex = 0;

            while (nLastIndex >= offset)
            {
                string_builder[nLastIndex] = ms_digits[UInt64_val % 10];
                UInt64_val /= 10;
                nLastIndex--;
                nCommasIndex++;
                if (nLastIndex > 0 && nCommasIndex % 3 == 0)
                {
                    string_builder[nLastIndex] = ',';
                    nLastIndex--;
                    nCommasIndex = 0;
                }
            }

            return string_builder;
        }

        public static StringBuilder Align(this StringBuilder string_builder, TextBeatAlign align)
        {
            string_builder.Length = string_builder.Capacity;

            int nLeftSpaceCount = 0;
            for (int i = 0; i < string_builder.Capacity; i++)
            {
                if (string_builder[i] == Placeholder)
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
                if (string_builder[i] == Placeholder)
                {
                    nRightSpaceCount = nRightSpaceCount + 1;
                }
                else
                {
                    break;
                }
            }

            if (align == TextBeatAlign.Center)
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
                        string_builder[string_builder.Capacity - 1 - nRightSpaceCount - i] = Placeholder;
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
                        string_builder[nLeftSpaceCount + i] = Placeholder;
                    }
                }
            }
            else if (align == TextBeatAlign.Right)
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
            else if (align == TextBeatAlign.Left)
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
            return Align(string_builder, TextBeatUtility.GetAlign(align));
        }

        public static StringBuilder Align(this StringBuilder string_builder, TMPro.TextAlignmentOptions align)
        {
            return Align(string_builder, TextBeatUtility.GetAlign(align));
        }


        public static string GetGarbageFreeString(this StringBuilder string_builder)
        {
            return (string)string_builder.GetType().GetField("_str", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(string_builder);
        }

        public static void GarbageFreeClear(this StringBuilder string_builder)
        {
            string_builder.Length = 0;
            string_builder.Append(Placeholder, string_builder.Capacity);
            string_builder.Length = 0;
        }
    }
}