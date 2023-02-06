#nullable enable
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityBase.DynamicUndo.Base;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace DynamicUndo
{
	[DefaultExecutionOrder(-1000)]
	public class UndoSuperManager : UndoManager<UndoManagerAction>
	{
		[Header("- UndoSuperManager -")]
		[SerializeField] [ReadOnly]
		private bool __childrenActionsEnabled;

		public                              List<GameObject> undoGameobjects = new();
		[SerializeField] [ReadOnly] private int              _undoManagerCount;

		[Header("Input action bindings")]
		[SerializeField]
		private InputActionReference? undoAction;

		[SerializeField] private InputActionReference? redoAction;

		private IUndoManager?      _executingManager;
		public  List<IUndoManager> undoManagers = new();

		public bool childrenActionsEnabled
		{
			set
			{
				__childrenActionsEnabled = value;
				foreach (var um in undoManagers) um.actionsEnabled = value;
			}
		}

		public override float undoRate => _executingManager!.undoRate;


		// --- Unity methods ---
		protected override void Awake()
		{
			_initializeUndoStack = false;
			base.Awake();
		}

		protected override void Start()
		{
			foreach (var o in undoGameobjects) AddUndoManagers(o.GetComponents<IUndoManager>());

			if (undoAction != null)
				undoAction.action.started += c => (this as IUndoManager).Undo();
			else
				Debug.LogWarning($"No undo action specified in {this}");
			if (redoAction != null)
				redoAction.action.started += c => (this as IUndoManager).Redo();
			else
				Debug.LogWarning($"No redo action specified in {this}");

			base.Start();
		}

		protected override void Update()
		{
			if (!UpdateProgress()) return;
			if (state == UndoStackManagerState.Inactive) {
				childrenActionsEnabled = true;
				_executingManager      = null;
			}
		}


		protected override void OnValidate()
		{
			_initializeUndoStack = false;
			base.OnValidate();
		}

		// --- IUndoManager ---
		public override bool Execute(UndoStackManagerState action)
		{
			if (!CanExecute(action)) return false;
			if (state == UndoStackManagerState.Inactive) {
				// find next UndoManager which can execute the action
				_undoStack.target_i = GetValidUMA_i(action);
				_executingManager   = target.undoManager;
			} // else pass action to currently executing UndoManager

			childrenActionsEnabled            = false;
			_executingManager!.actionsEnabled = true;
			// Try to execute found UndoManager: will fail at either end of the undo stack
			if (!_executingManager.Execute(action)) {
				childrenActionsEnabled = true;
				return false;
			}

			return true;
		}

		public override bool Push()
		{
			//throw new Exception("UndoSuperManager tracks other UndoManagers - Push() is ambiguous!");
			Debug.LogError("UndoSuperManager tracks other UndoManagers - Push() is ambiguous!");
			return false;
		}

		public override void Save()
		{
			base.Save();
			foreach (var um in undoManagers) um.Save();
		}

		public int GetValidUMA_i(UndoStackManagerState action)
		{
			Assert.IsTrue(action == UndoStackManagerState.Undo || action == UndoStackManagerState.Redo);
			var i = 0;
			for (i = _undoStack.target_i; i >= 0 && i < _undoStack.stack.Count(); i += (int)action)
				if (_undoStack.stack[i].undoManager.CanExecute(action))
					return i;
			return _undoStack.target_i;
		}


		// --- Event handlers for targetted UndoManagers ---
		private void OnPush(UndoManagerAction evt)
		{
			if (evt.oldSize == 0) {
				_undoStack.stack.Insert(0, evt);
				_undoStack.target_i++;
				FinalizePush(_undoStack.target_i - 1, _undoStack.stack.Count - 1);
				return;
			}

			// Delete newer events
			for (var i = _undoStack.stack.Count - 1; i > _undoStack.target_i; i--) {
				var el = _undoStack.stack[i];
				Assert.IsFalse(el.oldSize == 0);
				if (el.undoManager != evt.undoManager) el.undoManager.Resize(el.oldSize);
			}

			Push(evt);
		}

		private void OnAction(UndoManagerAction evt)
		{
			var i = FindEvent(evt);
			// Check that the action resulted in a state change -> anticipate target_i change in Execute()
			if (_undoStack.target_i == i && activationCounter == 0)
				_undoStack.target_i = i - (int)evt.action;

			base.Execute(evt.action);
			childrenActionsEnabled            = false;
			_executingManager!.actionsEnabled = true;
		}

		private void Compare(int x, int y)
		{
			Debug.Log($"x:{x} y:{y} x.compareTo(y):{x.CompareTo(y)} y.compareTo(x):{y.CompareTo(x)}");
		}

		private int FindEvent(UndoManagerAction evt)
		{
			// TODO: handle missing events (ensure inStack_i > 0)
			var inStack_i =
				_undoStack.stack.FindLastIndex(x => x.newTarget_i == evt.newTarget_i && x.undoManager == evt.undoManager);
			Assert.IsTrue(inStack_i >= 0, $"Failed to find event: {evt}");

			//Debug.Log($"[{_undoStack.target_i}->{inStack_i}/{_undoStack.stack.Count - 1}] {evt}");
			return inStack_i;
		}

		public virtual void AddUndoManagers(IEnumerable<IUndoManager> ums)
		{
			foreach (var um in ums) AddUndoManager(um);
		}

		public virtual void AddUndoManager(IUndoManager um)
		{
			Assert.IsFalse(um.managed, "Cannot add an already managed UndoManager (or another UndoSuperManager)!");
			undoManagers.Add(um);
			um.managed        =  true;
			um.pushed         += OnPush;
			um.executed       += OnAction;
			_undoManagerCount =  undoManagers.Count;
		}
	}
}
