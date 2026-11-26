using Reflex.Core;
using UnityEngine;

public class KeyValueStoreInstaller : MonoBehaviour, IInstaller
{
	public void InstallBindings(ContainerBuilder builder)
	{
		builder.AddSingleton(typeof(PlayerPrefsKeyValueStore), typeof(IKeyValueStore));
		builder.AddSingleton(typeof(PlayerContextKeyValueStore), typeof(IPlayerAwareKeyValueStore));
	}
}
