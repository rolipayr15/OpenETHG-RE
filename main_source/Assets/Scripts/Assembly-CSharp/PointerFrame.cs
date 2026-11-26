using UnityEngine;

public struct PointerFrame
{
	public int Frame;

	public Vector2 RelativeDelta;

	public Vector2 PointerPosition;

	public Vector2 PointerDelta;

	public Vector2 Scroll;

	public uint HeldButtons;

	public uint DownButtons;

	public uint UpButtons;

	public ulong HeldKeys;

	public ulong DownKeys;

	public ulong UpKeys;

	public bool HasPosition;

	public bool ButtonHeld(int index)
	{
		return false;
	}

	public bool ButtonDown(int index)
	{
		return false;
	}

	public bool ButtonUp(int index)
	{
		return false;
	}

	public bool KeyHeld(int index)
	{
		return false;
	}

	public bool KeyDown(int index)
	{
		return false;
	}
}
