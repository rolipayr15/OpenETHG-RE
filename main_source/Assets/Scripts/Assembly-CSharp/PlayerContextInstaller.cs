using Reflex.Core;
using UnityEngine;

public class PlayerContextInstaller : MonoBehaviour, IInstaller
{
	public void InstallBindings(ContainerBuilder builder)
	{
		builder.AddSingleton(typeof(PlayerContextManager), typeof(IPlayerContextManager));
	}
}
