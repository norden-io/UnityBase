#nullable enable
using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityBase.Config;
using UnityBase.DynamicUndo.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace DynamicUndo
{
	public abstract class UndoManager<T> : MonoBehaviour, IUndoManager where T : IUndoElement, new()
	{
		// Stack data
		[Foldout("UndoManager state")]
		[SerializeField] internal UndoStack<T> _undoStack = new();

		[Foldout("UndoManager state")]
		[Tooltip("Automatically Push during Start if Undo Stack is empty")]
		[SerializeField] internal bool _initializeUndoStack = true;
		
		[Foldout("UndoManager state")]
		[Tooltip("Enable/disable Undo, Redo and Push")]
		[SerializeField] private bool _actionsEnabled = true;
		
		[Foldout("UndoManager state")]
		[SerializeField] [ReadOnly] internal UndoStackManagerState _state = UndoStackManagerState.Inactive;

		[Foldout("UndoManager state")]
		[SerializeField] [ReadOnly] private float _currentRate, _progress;

		[Foldout("UndoManager state")]
		[Tooltip("Counts how many times undo/redo have been pressed during current undo/redo execution.")]
		[SerializeField] [ReadOnly] private int _activationCounter;

		[Foldout("UndoManager state")]
		[Tooltip("Tracks whether this UndoManager is managed by a UndoSuperManager - do not change manually")]
		[SerializeField] private bool _managed;

		// Inspector debug inputs
		[Foldout("UndoManager action buttons for debugging")]
		[SerializeField] private bool _save, _undo, _redo, _push;

		//public Model.TransientData stackData => _undoStack;
		private int _capacity;

		// Overrides


		// Constructors
		public UndoManager() : this(-1)
		{
		}

		public UndoManager(int capacity, bool initialize = true, string? loadPath = null)
		{
			_capacity            = capacity;
			_initializeUndoStack = initialize;
			// If loadPath is not explicitly set, wait until Awake() to Initialize()
			if (!string.IsNullOrEmpty(loadPath)) Initialize(loadPath);
		}

		protected virtual T     target      => _undoStack.target;
		protected virtual T     source      => _undoStack.stack[_undoStack.target_i - (int)_state];
		public virtual    float undoSpeedup { get; } = 2;

		public virtual int undoMaxStates { get; } = -1;


		// Unity methods
		protected virtual void Awake()
		{
			if (_capacity == -1) _capacity = undoMaxStates;
			// Don't Initialize again if already initialized (loadPath set in constructor)
			if (!_undoStack.loaded) Initialize(_undoStack.dataPath);
			_undoStack.stack.Capacity = _capacity + 1;
		}

		protected virtual void Start()
		{
			if (_initializeUndoStack && _undoStack.stack.Count == 0)
				Push();
		}

		// Inherited classes must implement animations using UpdateProgress()
		protected abstract void Update();

		protected virtual void OnValidate()
		{
			if (_undo) {
				_undo = false;
				if (!((IUndoManager)this).Undo()) Debug.LogWarning($"Failed to Undo {this}");
			}
			else if (_redo) {
				_redo = false;
				if (!((IUndoManager)this).Redo()) Debug.LogWarning($"Failed to Redo {this}");
			}
			else if (_push) {
				_push = false;
				if (!Push()) Debug.LogWarning($"Failed to Push {this}");
			}
			else if (_save) {
				_save = false;
				Save();
			}
		}

		public MonoBehaviour         monoBehaviour => this;
		public UndoStackManagerState state         => _state;
		public float                 progress      => _progress;

		public bool actionsEnabled
		{
			get => _actionsEnabled;
			set => _actionsEnabled = value;
		}

		public int activationCounter => _activationCounter;

		public bool managed
		{
			get => _managed;
			set => _managed = value;
		}

		// Events
		public event PushEventHandler? pushed;
		public event RedoEventHandler? executed;
		public virtual float           undoRate { get; } = 3;

		public virtual bool Execute(UndoStackManagerState action)
		{
			Assert.IsTrue(action == UndoStackManagerState.Undo || action == UndoStackManagerState.Redo);
			if (!CanExecute(action)) return false;
			var oldTarget_i = _undoStack.target_i;

			if (state == UndoStackManagerState.Inactive) {
				_progress = 0;
				ChangeState(action);
			}
			else if (state == action) {
				_currentRate *= undoSpeedup;
			}
			else /* state != action */ {
				_progress = 1 - _progress;
				ChangeState(action);
			}

			executed?.Invoke(new UndoManagerAction(this, oldTarget_i, _undoStack.target_i, _undoStack.stack.Count));
			_activationCounter++;
			return true;
		}

		public virtual bool CanExecute(UndoStackManagerState action)
		{
			if (!actionsEnabled) return false;
			if (state == UndoStackManagerState.Inactive) {
				var new_i = _undoStack.target_i + (int)action;
				if (new_i < 0 || new_i > _undoStack.stack.Count - 1)
					return false;
			}

			return true;
		}

		public virtual bool Push()
		{
			if (!actionsEnabled) return false;
			if (state != UndoStackManagerState.Inactive) return false; // TODO: allow state save during undo/redo?

			var element = new T();
			element.GetDataFrom(gameObject);
			return Push(element);
		}

		public virtual void Save()
		{
			_undoStack.Save();
		}

		public virtual void Resize(int size)
		{
			_undoStack.target_i = Math.Min(size - 1, _undoStack.target_i);
			for (var i = _undoStack.stack.Count - 1; i >= size; i--) _undoStack.stack.RemoveAt(i);
		}

		private void Initialize(string? loadPath)
		{
			// Creating new UndoManager: do not load, just initialize stack
			if (string.IsNullOrEmpty(loadPath)) {
				_undoStack.stack = new List<T>(Math.Max(_capacity, 0));
				return;
			}

			// Loading
			var loaded = TransientData.Load<UndoStack<T>>(loadPath);
			if (loaded.dataPath != loadPath) {
				// first time initialization (saved file not found)
				if (!_initializeUndoStack) _undoStack.Save();
			}
			else {
				// use loaded data
				_undoStack = loaded;
				if (_undoStack.target_i > 0) {
					// load saved state	TODO: save state on program load
					_state    = UndoStackManagerState.Redo;
					_progress = 1;
				}
				//_target_i = _undoStack.stack.Count - 1;
			}
		}

		public bool Push(T element)
		{
			var oldTarget_i = _undoStack.target_i;
			var oldSize     = _undoStack.stack.Count;

			// Clear any possible redo actions
			for (var i = _undoStack.stack.Count - 1; i > _undoStack.target_i; i--) _undoStack.stack.RemoveAt(i);
			// Add new undo state to stack
			_undoStack.stack.Add(element);
			// Check if state capacity has been exceeded
			if (_capacity >= 0 && _undoStack.stack.Count > _capacity)
				throw new NotImplementedException(
					"UndoStack capacity exceeded, but Pop behaviour has not been implemented!");
			// TODO: Pop() that emits an event so UndoSuperManager can update
			//_undoStack.stack.RemoveAt(0);
			_undoStack.target_i = _undoStack.stack.Count - 1;
			FinalizePush(oldTarget_i, oldSize);
			return true;
		}

		protected void FinalizePush(int oldTarget_i, int oldSize)
		{
			_undoStack.Save();
			pushed?.Invoke(new UndoManagerAction(this, oldTarget_i, _undoStack.target_i, oldSize, _undoStack.stack.Count));
		}


		// Non-public methods
		private void ChangeState(UndoStackManagerState newState)
		{
			_undoStack.target_i += (int)newState;
			_currentRate        =  undoRate;
			_state              =  newState;
		}

		protected bool UpdateProgress()
		{
			if (_state == UndoStackManagerState.Inactive) return false;

			_progress += Time.deltaTime * _currentRate;
			if (_progress >= 1) {
				_state             = UndoStackManagerState.Inactive;
				_progress          = 1;
				_activationCounter = 0;
			}

			return true;
		}
	}
}
