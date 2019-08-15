using System;
using System.Collections.Generic;

namespace Engine.Structures
{
	public class SafeList<T> where T : IDisposable
	{
		private List<T> addList;
		private List<T> removeList;
		private List<T> mainList;

		public SafeList()
		{
			addList = new List<T>();
			removeList = new List<T>();
			mainList = new List<T>();
		}

		public void Add(T item)
		{
			addList.Add(item);
		}

		public void Remove(T item)
		{
			removeList.Add(item);
		}

		public List<T> MainList => mainList;

		public void ProcessChanges()
		{
			// Adding first ensures that if an item is added and removed before changes are processed, it will be
			// correctly removed.
			if (addList.Count > 0)
			{
				mainList.AddRange(addList);
				addList.Clear();
			}

			if (removeList.Count > 0)
			{
				foreach (T item in removeList)
				{
					mainList.Remove(item);
				}

				removeList.Clear();
			}
		}
	}
}
