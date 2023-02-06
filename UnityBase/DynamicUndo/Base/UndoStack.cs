#nullable enable
using System;
using System.Collections.Generic;
using UnityBase.Config;

namespace UnityBase.DynamicUndo.Base
{
	[Serializable]
	public class UndoStack<T> : TransientData where T : IUndoElement?, new()
	{
		public List<T>
			stack = new(); // TODO: use linked list (NB: UndoManager OnValidate must handle changing _target_i then!)

		public int target_i = -1;

		public T target => stack[target_i];
		/*
		internal T next => target_i < stack.Count - 1 ? stack[target_i + 1] : target;
		internal T prev => target_i > 0 ? stack[target_i - 1] : target;
		*/
	}
}
