﻿namespace Uno.Extensions.Navigation.Toolkit;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddToolkitNavigation(
		this IServiceCollection services)
	{
		return services
					.AddTransient<Flyout, ModalFlyout>()

					.AddRegion<TabBar, TabBarNavigator>()

					.AddSingleton<IRequestHandler, TabBarItemRequestHandler>();
	}
}
