using System.Collections;
using UnityEngine;

namespace NullPointerCore.Backend.Commands
{
	/// <summary>
	/// Base abstract class to control the command execution.
	/// </summary>
	public abstract class Command
	{
		public enum State
		{
			Wait,
			Run,
			Cancel,
			End,
		}
		private State state = State.Wait;
		private GameObject context;

		/// <summary>
		/// The current execution state of this command.
		/// </summary>
		public State CurrentState { get { return state; } }
		/// <summary>
		/// The GameObject that acts as a central context of execution for this command.
		/// </summary>
		public GameObject Context { get {return context; } }

		protected virtual IEnumerator OnStarted() { return null; } 
		protected virtual void OnUpdate(float time) { }
		protected virtual IEnumerator OnCanceled() { return null; }
		protected virtual IEnumerator OnEnded() { return null; }

		public void Cancel()
		{
			if(state==State.Run)
				state = State.Cancel;
			else if(state==State.Wait)
				state = State.End;
		}
		public void End()
		{
			state = State.End;
		}

		internal IEnumerator Run(GameObject context)
		{
			if(CurrentState != Command.State.Wait )
				yield break;

			this.context = context;
			this.state = State.Run;
			yield return OnStarted();

			while(CurrentState == Command.State.Run  )
			{
				OnUpdate(Time.deltaTime);
				yield return null;
			}
			if (CurrentState == Command.State.Cancel)
			{
				yield return OnCanceled();
				End();
			}
			if (CurrentState == Command.State.End)
				yield return OnEnded();
		}
	}

	/// <summary>
	/// Custom Command that controls specific components of the gameobject context
	/// </summary>
	/// <typeparam name="T">The component Type to be controlled by this class.</typeparam>
	public abstract class Command<T> : Command where T : Component
	{
		private T target;

		/// <summary>
		/// The Component Target controlled by this Command.
		/// </summary>
		public T ComponentTarget { get { return target; } }

		protected override IEnumerator OnStarted()
		{
			target = Context.GetComponent<T>();
			if(target==null)
			{
				Debug.LogError("There is no component of type "+typeof(T).Name +"in this gameObject "+Context.name );
				End();
			}
			return base.OnStarted();
		}
	}
}