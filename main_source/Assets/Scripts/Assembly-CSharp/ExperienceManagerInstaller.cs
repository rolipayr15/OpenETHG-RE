using Reflex.Injectors;
using Reflex.Core;
using UnityEngine;

public class ExperienceManagerInstaller : MonoBehaviour, IInstaller
{
	public void InstallBindings(ContainerBuilder builder)
	{
		builder.AddSingleton<IExpFormulaProvider>((Container container) => new QuadraticCappedExpFormulaProvider());
		builder.AddSingleton<IExperienceManager>((Container container) => new ExperienceManager(new PlayerContextKeyValueStore(container.Single<IKeyValueStore>(), container.Single<IPlayerContextManager>()), container.Single<IExpFormulaProvider>()));
	}
}
