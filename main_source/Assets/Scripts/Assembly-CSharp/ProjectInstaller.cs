using Reflex.Injectors;
using EyesAnalytics;
using Multiplayer.ExternalApi;
using Multiplayer.Chat;
using GameAuthentication;
using Eyes.DailyChallenges;
using Reflex.Core;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
	public void InstallBindings(ContainerBuilder builder)
	{
		builder.AddSingleton(typeof(ApiDailyChallengeAssignmentProvider), typeof(IDailyChallengeAssignmentProvider));
		builder.AddSingleton((Container container) => new DailyChallengeService(container.Single<IPlayerAwareKeyValueStore>(), container.Single<IPlayerContextManager>(), container.Single<IDailyChallengeAssignmentProvider>(), null, CoinsManager.AddCoinsForDailyChallenge, EyesAnalytics.DailyChallenges.Completed));
		builder.OnContainerBuilt += delegate(Container container)
		{
			DailyChallengeRuntime.Configure(container.Single<DailyChallengeService>());
		};
		builder.AddSingleton(typeof(GameSingleApi), typeof(IGameSingleApi));
		builder.AddSingleton(typeof(ExpApi), typeof(IExpApi));
		builder.AddSingleton(typeof(UserApi), typeof(IUserApi));
		builder.AddSingleton(typeof(StatsApi), typeof(IStatsApi));
		builder.AddSingleton(typeof(ChallengesApi), typeof(IChallengesApi));
		builder.AddSingleton(typeof(ChatClient), typeof(IChatClient));
		builder.AddSingleton(typeof(AuthBootstrapStore), typeof(IAuthBootstrapStore));
		builder.AddSingleton(typeof(UgsGameAccountGateway), typeof(IUgsGameAccountGateway));
		builder.AddSingleton(typeof(UnityPlayerAccountProviderAdapter), typeof(IAuthProviderAdapter));
		builder.AddSingleton(typeof(FacebookAuthProviderAdapter), typeof(IAuthProviderAdapter));
		builder.AddSingleton(typeof(AppleAuthProviderAdapter), typeof(IAuthProviderAdapter));
		builder.AddSingleton(typeof(GooglePlayGamesAuthProviderAdapter), typeof(IAuthProviderAdapter));
		builder.AddSingleton(typeof(AuthCoordinator), typeof(IAuthCoordinator));
	}
}
