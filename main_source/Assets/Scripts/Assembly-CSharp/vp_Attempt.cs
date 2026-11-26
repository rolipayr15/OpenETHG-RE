using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class vp_Attempt : vp_Event
{
	public delegate bool Tryer();

	public Tryer Try;

	protected static bool AlwaysOK()
	{
		return true;
	}

	public vp_Attempt(string name)
		: base(name)
	{
		InitFields();
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { base.Type.GetField("Try") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { base.Type.GetMethod("AlwaysOK") };
		m_DelegateTypes = new Type[1] { typeof(Tryer) };
		Prefixes = new Dictionary<string, int> { { "OnAttempt_", 0 } };
		Try = AlwaysOK;
	}

	public override void Register(object t, string m, int v)
	{
		Try = (Tryer)Delegate.CreateDelegate(m_DelegateTypes[v], t, m);
		Refresh();
	}

	public override void Unregister(object t)
	{
		Try = AlwaysOK;
		Refresh();
	}
}
public class vp_Attempt<V> : vp_Attempt
{
	public new delegate bool Tryer<T>(T value);

	public new Tryer<V> Try;

	protected static bool AlwaysOK<T>(T value)
	{
		return true;
	}

	public vp_Attempt(string name)
		: base(name)
	{
	}

	protected override void InitFields()
	{
		m_Fields = new FieldInfo[1] { base.Type.GetField("Try") };
		StoreInvokerFieldNames();
		m_DefaultMethods = new MethodInfo[1] { GetStaticGenericMethod(base.Type, "AlwaysOK", m_ArgumentType, typeof(bool)) };
		m_DelegateTypes = new Type[1] { typeof(Tryer<>) };
		Prefixes = new Dictionary<string, int> { { "OnAttempt_", 0 } };
		if (m_DefaultMethods != null && m_DefaultMethods[0] != null)
		{
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));
		}
	}

	public override void Register(object t, string m, int v)
	{
		if (((Delegate)m_Fields[v].GetValue(this)).Method.Name != m_DefaultMethods[v].Name)
		{
			Debug.LogWarning("Warning: Event '" + base.EventName + "' of type (vp_Attempt) targets multiple methods. Events of this type must reference a single method (only the last reference will be functional).");
		}
		if (m != null)
		{
			SetFieldToExternalMethod(t, m_Fields[0], m, MakeGenericType(m_DelegateTypes[v]));
		}
	}

	public override void Unregister(object t)
	{
		if (m_DefaultMethods != null && m_DefaultMethods[0] != null)
		{
			SetFieldToLocalMethod(m_Fields[0], m_DefaultMethods[0], MakeGenericType(m_DelegateTypes[0]));
		}
	}
}
