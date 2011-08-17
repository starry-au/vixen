﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Vixen.Module.Input {
	/// <summary>
	/// Base class for trigger module implementations.
	/// </summary>
	abstract public class InputModuleInstanceBase : ModuleInstanceBase, IInputModuleInstance, IEquatable<InputModuleInstanceBase>, IEquatable<IInputModuleInstance>, IEqualityComparer<IInputModuleInstance>, IEqualityComparer<InputModuleInstanceBase> {
		private Thread _stateUpdateThread;
		private ManualResetEvent _pause = new ManualResetEvent(true);

		public event EventHandler<InputValueChangedEventArgs> InputValueChanged;

		abstract public IInputInput[] InputInputs { get; }

		abstract public void UpdateState();

		public bool Enabled { get; set; }

		public void Start() {
			if(!IsRunning) {
				// Call the subclass first in case its startup creates the triggers.
				DoStartup();
				// Subscribe to triggers.
				foreach(IInputInput input in InputInputs) {
					input.ValueChanged += _InputValueChanged;
				}
				// Start monitoring the hardware.
				_stateUpdateThread = new Thread(_StateUpdate);
				_stateUpdateThread.IsBackground = true;
				_stateUpdateThread.Start();
			}
		}

		protected virtual void DoStartup() { }

		public void Stop() {
			// In opposite order of Startup...
			if(IsRunning) {
				// Stop monitoring the hardware.
				Resume();
				IsRunning = false;
				// Unsubscribe to triggers.
				foreach(IInputInput input in InputInputs) {
					input.ValueChanged -= _InputValueChanged;
				}
				// Notify the subclass.
				DoShutdown();
			}
		}

		protected virtual void DoShutdown() { }

		public void Pause() {
			if(!IsPaused) {
				IsPaused = true;
				_pause.Reset();
				DoPause();
			}
		}

		protected virtual void DoPause() { }

		public void Resume() {
			if(IsPaused) {
				IsPaused = false;
				_pause.Set();
				DoResume();
			}
		}

		protected virtual void DoResume() { }

		public bool IsRunning { get; private set; }

		public bool IsPaused { get; private set; }

		abstract public bool Setup();

		override public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			Stop();
			_pause.Dispose();
			_pause = null;
		}

		~InputModuleInstanceBase() {
			Dispose(false);
		}

		private void _InputValueChanged(object sender, EventArgs e) {
			if(InputValueChanged != null) {
				InputValueChanged(this, new InputValueChangedEventArgs(sender as IInputInput));
			}
		}

		private void _StateUpdate() {
			IsRunning = true;
			while(IsRunning) {
				UpdateState();
				Thread.Sleep(5);
				_pause.WaitOne();
			}
			_stateUpdateThread = null;
		}

		public bool Equals(InputModuleInstanceBase other) {
			return Equals(other as IInputModuleInstance);
		}

		public bool Equals(IInputModuleInstance other) {
			return base.Equals(this, other);
		}

		public bool Equals(IInputModuleInstance x, IInputModuleInstance y) {
			return base.Equals(x, y);
		}

		public int GetHashCode(IInputModuleInstance obj) {
			return base.GetHashCode(obj);
		}

		public bool Equals(InputModuleInstanceBase x, InputModuleInstanceBase y) {
			return Equals(x as IInputModuleInstance, y as IInputModuleInstance);
		}

		public int GetHashCode(InputModuleInstanceBase obj) {
			return GetHashCode(obj as IInputModuleInstance);
		}
	}
}