using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextBeat
{
    internal interface InterfaceCanRecycleObj
    {
		void Clear();
    }

	internal static class ObjectPool<T> where T : InterfaceCanRecycleObj, new()
	{
		private static Queue<T> mPoolQueue = new Queue<T>();

		public static void recycle(T array)
		{
			array.Clear();
			mPoolQueue.Enqueue(array);
		}

		public static T Pop()
		{
			if (mPoolQueue.Count == 0)
			{
				return new T();
			}else
            {
				return mPoolQueue.Dequeue();
            }
		}

		public static void release()
		{
			mPoolQueue.Clear();
		}
	}
}
