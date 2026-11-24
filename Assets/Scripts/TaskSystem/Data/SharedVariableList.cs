using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

namespace Game.Cozyffee.TaskSystem
{
	public class SharedVariableList<T>
	{
		private List<SharedVariable<T>> list = new();

		private Dictionary<string, SharedVariable<T>> dictionary = new();

		public IReadOnlyList<SharedVariable<T>> List => list.AsReadOnly();

		public void Set(string name, SharedVariable<T> v)
		{
			v.Name = name;

			if (dictionary.ContainsKey(name))
			{
				dictionary[name] = v;
				list.Remove(v);
				list.Add(v);
			}
			else
			{
				dictionary.Add(name, v);
				list.Add(v);
			}
		}

		public void Add(SharedVariable<T> v)
		{
			if (string.IsNullOrWhiteSpace(v.Name))
			{
				Debug.LogError("Added a shared variable to BT data with no name");
				return;
			}

			if (dictionary.ContainsKey(v.Name))
			{
				Debug.LogError($"Variable named '{v.Name}' already exists in BT data!");
				return;
			}

			dictionary.Add(v.Name, v);
			list.Add(v);
		}

		public void Remove(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				Debug.LogError("Attempted to remove a variable without providing a name");
				return;
			}

			list.Remove(dictionary[name]);
			dictionary.Remove(name);
		}

		public SharedVariable<T> Get(string name)
		{
			if (dictionary.TryGetValue(name, out var variable))
			{
				return variable;
			}

			Debug.LogError($"Variable named '{name}' not found in BT data!");

			return null;
		}
	}
}