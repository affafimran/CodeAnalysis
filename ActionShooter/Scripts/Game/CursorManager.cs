using UnityEngine;
using System.Collections;

/// <summary>
/// Simple cursor manager, so we don't have to call the same code all over the place
/// Not much to do here though
/// </summary>
public static class CursorManager
{
	// default Set
	// we always either lock and hide OR unlock and show
	public static void SetCursor(bool aState)
	{
		// [MOBILE] Don't turn off the cursor, this is hard to develop ;)
		if (GameData.mobile) return;
		Cursor.lockState = (aState) ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = aState;
	}
}

