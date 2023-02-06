#nullable enable
using UnityEngine;

namespace UnityBase.DynamicUndo.Base
{
	public enum UndoStackManagerState
	{
		Undo     = -1,
		Inactive = 0,
		Redo     = 1
	}

	public delegate void PushEventHandler(UndoManagerAction context);

	public delegate void UndoEventHandler(UndoManagerAction context);

	public delegate void RedoEventHandler(UndoManagerAction context);

	public interface IUndoManager
	{
		public GameObject    gameObject    { get; }
		public MonoBehaviour monoBehaviour { get; }

		//public Model.TransientData stackData { get; }
		public UndoStackManagerState state             { get; }
		public float                 progress          { get; }
		public bool                  actionsEnabled    { get; set; }
		public int                   activationCounter { get; }
		public bool                  managed           { get; set; }

		public float undoRate { get; }

		public event PushEventHandler pushed;
		public event RedoEventHandler executed;

		public bool Execute(UndoStackManagerState    action);
		public bool CanExecute(UndoStackManagerState action);

		public bool Redo()
		{
			return managed ? false : Execute(UndoStackManagerState.Redo);
		}

		public bool CanRedo()
		{
			return managed ? false : CanExecute(UndoStackManagerState.Redo);
		}

		public bool Undo()
		{
			return managed ? false : Execute(UndoStackManagerState.Undo);
		}

		public bool CanUndo()
		{
			return managed ? false : CanExecute(UndoStackManagerState.Undo);
		}

		public bool Push();
		public void Save();
		public void Resize(int size);
	}
}
