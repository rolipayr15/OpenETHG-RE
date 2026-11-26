using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AndroidPermissionCallback : AndroidJavaProxy
{
	[CompilerGenerated]
	private Action<string> m_OnPermissionGrantedAction;

	[CompilerGenerated]
	private Action<string> m_OnPermissionDeniedAction;

	private event Action<string> OnPermissionGrantedAction
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	private event Action<string> OnPermissionDeniedAction
	{
		[CompilerGenerated]
		add
		{
		}
		[CompilerGenerated]
		remove
		{
		}
	}

	public AndroidPermissionCallback(Action<string> onGrantedCallback, Action<string> onDeniedCallback)
		: base((string)null)
	{
	}

	public virtual void OnPermissionGranted(string permissionName)
	{
	}

	public virtual void OnPermissionDenied(string permissionName)
	{
	}
}
