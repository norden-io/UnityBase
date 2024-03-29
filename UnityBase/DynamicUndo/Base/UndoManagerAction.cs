﻿#nullable enable
using System;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace UnityBase.DynamicUndo.Base
{
	[Serializable]
	public struct UndoManagerAction : IUndoElement, ISerializationCallbackReceiver
	{
		[SerializeField]            private string? _name;
		[SerializeField] [ReadOnly] private string? _globalObjectIdString;

		// Action data
		public   int            newTarget_i, oldTarget_i, newSize, oldSize;
#if UNITY_EDITOR // for Unity Inspector
		internal GlobalObjectId _globalObjectId;
		[SerializeField]
#endif
		[ReadOnly]
		private IUndoManager? _undoManager;

		[NonSerialized] public UndoStackManagerState action;

		public UndoManagerAction(IUndoManager undoManager, int oldTarget_i, int newTarget_i, int oldSize,
			int                                newSize = -1)
		{
			_undoManager          = undoManager;
			action                = undoManager.state;
#if UNITY_EDITOR
			_globalObjectId       = GlobalObjectId.GetGlobalObjectIdSlow(undoManager as MonoBehaviour);
#endif
			_name                 = null;
			_globalObjectIdString = null;
			this.oldTarget_i      = oldTarget_i;
			this.newTarget_i      = newTarget_i;
			this.oldSize          = oldSize;
			this.newSize          = newSize < 0 ? oldSize : newSize;
		}

		// UndoManager object indetifiers
		public IUndoManager undoManager
		{
			get
			{
				if (_undoManager == null) {
#if UNITY_EDITOR
					_undoManager = (GlobalObjectId.GlobalObjectIdentifierToObjectSlow(_globalObjectId) as IUndoManager)!;
#endif

					if (_undoManager == null) throw new Exception($"Failed to get UndoManager for {_name}");
				}

				return _undoManager;
			}
		}

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			_globalObjectIdString = _globalObjectId.ToString();
#endif
			_name                 = undoManager.monoBehaviour.ToString();
		}

		public void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			if (!GlobalObjectId.TryParse(_globalObjectIdString, out _globalObjectId))
				Debug.LogError($"UndoManagerAction dailed to parse GlobalObjectId for {_name}");
#endif
		}

		public void GetDataFrom(GameObject o)
		{
			throw new NotImplementedException("Invalid for UndoManagerAction");
		}

		public override string ToString()
		{
			return
				$"[UndoManagerAction] target_i:{oldTarget_i}->{newTarget_i} size:{oldSize}->{newSize} in {_undoManager}";
		}
	}
}
