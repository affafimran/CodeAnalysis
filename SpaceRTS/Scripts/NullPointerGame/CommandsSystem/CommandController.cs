using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace NullPointerCore.Backend.Commands
{
	/// <summary>
	/// Class that receives custom commands to be applied to the GameEntity.
	/// This is just a first implementation of what can be a bigger feature.
	/// </summary>
	public class CommandController : MonoBehaviour
	{
		private List<Command> queuedCommands = new List<Command>();

		private IEnumerator currentRun = null;
		private Command currentCommand = null;

		/// <summary>
 		/// Returns the current executed command
		/// </summary>
		public Command CurrentCommand { get { return currentCommand; } }


		/// <summary>
		/// Indicates if there is pending queued commands (running or waiting to run)
		/// </summary>
		public bool HasPendingCommands { get { return queuedCommands.Count>0; } }

		/// <summary>
		/// Core loop for the command controller system
		/// </summary>
		/// <returns></returns>
		IEnumerator CommandsUpdate()
		{
			while(queuedCommands.Count > 0)
			{
				currentCommand = queuedCommands[0];
				yield return currentCommand.Run(gameObject);
				queuedCommands.RemoveAt(0);
			}
			currentCommand = null;
			currentRun = null;
		}

		/// <summary>
		/// Stop or cancel all current commands and appends a new one to run.
		/// </summary>
		/// <param name="command">the command to run after all the queued where stopped.</param>
		public void Set(Command command)
		{
			Assert.IsNotNull(command, "Invalid parameter. command is null.");

			Stop();
			Append(command);
		}

		/// <summary>
		/// Adds a new command to the queue. The added command needs to wait all previously queued 
		/// commands to normally ends its execution before to start.
		/// </summary>
		/// <param name="command"></param>
		public void Append(Command command)
		{
			Assert.IsNotNull(command, "Invalid parameter. command is null.");

			queuedCommands.Add(command);
			RunCommands();
		}

		/// <summary>
		/// Cancels the current command execution.
		/// </summary>
		public void CancelCurrent()
		{
			if(currentCommand!=null)
				currentCommand.Cancel();
		}

		/// <summary>
		/// Stops all queued commands.
		/// </summary>
		public void Stop()
		{
			foreach(Command command in queuedCommands)
				command.Cancel();
		}

		/// <summary>
		/// Checks if there are remaining commands to be executed and starts the execution.
		/// </summary>
		private void RunCommands()
		{
			if(!isActiveAndEnabled)
				return;
			// Its already running?
			if(currentRun != null)
				return;
			// There is not sense in execute anything if there 
			// is no commands in the queue.
			if(queuedCommands.Count==0)
				return;
			currentRun = CommandsUpdate();
			StartCoroutine(currentRun);
		}
	}
}
