
using System.Collections.Generic;
using UnityEngine;

namespace NinthArt
{
	internal class ObjectPool<T> where T : MonoBehaviour
	{
		private Stack<T> _inactive;
		private T _prefab;
		internal int InstanceCount { get; private set; }

		internal ObjectPool(T prefab, int capacity)
		{
			Init(prefab, capacity);
		}

		private void Init(T prefab, int capacity)
		{
			_prefab = prefab;
			_inactive = new Stack<T>(capacity);
		}

		internal T Spawn(Transform parent)
		{
			InstanceCount++;
			var obj = _inactive.Count > 0 ? _inactive.Pop() : Object.Instantiate(_prefab, parent);
			return obj;
		}

		internal void Despawn(T obj)
		{
			InstanceCount--;
			_inactive.Push(obj);
		}

		internal void DestroyObjects()
		{
			foreach (var obj in _inactive)
			{
				Object.Destroy(obj.gameObject);
			}
			_inactive.Clear();
		}
	}
}