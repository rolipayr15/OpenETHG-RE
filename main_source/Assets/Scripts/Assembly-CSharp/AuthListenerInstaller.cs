using Reflex.Injectors;
using Reflex.Core;
using UnityEngine;

public class AuthListenerInstaller : MonoBehaviour, IInstaller
{
	public void InstallBindings(ContainerBuilder builder)
	{
		if (BuildFlags.IsTestPlayerIdLabelEnabled)
		{
			SetupTestPlayerIdLabel(builder);
		}
		if (BuildFlags.IsTestAuthListenerEnabled)
		{
			SetupTestAuthInstaller(builder);
		}
		builder.AddSingleton(typeof(UnityAuthenticationListener), typeof(IAuthListener));
	}

	private void SetupTestAuthInstaller(ContainerBuilder builder)
	{
		builder.AddSingleton<IAuthListener>(delegate(Container container)
		{
			GameObject gameObject = new GameObject("TestAuthListener");
			TestAuthListener listener = gameObject.AddComponent<TestAuthListener>();
			GameObjectInjector.InjectObject(gameObject, container);
			return listener;
		});
	}

	private void SetupTestPlayerIdLabel(ContainerBuilder builder)
	{
		builder.AddSingleton(delegate(Container container)
		{
			GameObject gameObject = new GameObject("PlayerIdTestLabel");
			PlayerIdTestLabel label = gameObject.AddComponent<PlayerIdTestLabel>();
			GameObjectInjector.InjectObject(gameObject, container);
			return label;
		});
		builder.OnContainerBuilt += delegate(Container container)
		{
			container.Resolve<PlayerIdTestLabel>();
		};
	}
}
