﻿namespace Uno.Extensions.Hosting;

/// <summary>
/// Contains helpers to create a HostBuilder that is tailored to multiple target platforms.
/// </summary>
public static class UnoHost
{

	/// <summary>
	/// Obsolete; use <see cref="CreateDefaultBuilder(Assembly, System.String[])"/> or
	/// <see cref="CreateDefaultBuilder{TApplication}(System.String[])"/>.
	/// </summary>
	/// <param name="args">
	/// The command line arguments.
	/// </param>
	/// <returns>
	/// The initialized IHostBuilder.
	/// </returns>
	[Obsolete("Use CreateDefaultBuilder(Assembly, string[]) or CreateDefaultBuilder<TApplication>(string[]) instead.")]
	public static IHostBuilder CreateDefaultBuilder(string[]? args = null)
	{
		return CreateDefaultBuilder(PlatformHelper.GetAppAssembly()!, args);
	}

	/// <summary>
	/// Initializes a new instance of the HostBuilder class that is pre-configured 
	/// for multi-platform Uno applications.
	/// </summary>
	/// <typeparam name="TApplication">
	/// The type of the main Application.  Used to determine the App's <see cref="Assembly" />.
	/// </typeparam>
	/// <param name="args">
	/// The command line arguments.
	/// </param>
	/// <returns>
	/// The initialized IHostBuilder.
	/// </returns>
	public static IHostBuilder CreateDefaultBuilder<TApplication>(string[]? args = null)
		where TApplication : Application
	{
		var callingAssembly = typeof(TApplication).Assembly;
		return CreateDefaultBuilder(callingAssembly, args);
	}

	/// <summary>
	/// Initializes a new instance of the HostBuilder class that is pre-configured
	/// for multi-platform Uno applications.
	/// </summary>
	/// <param name="applicationAssembly">
	/// The application <see cref="Assembly"/>.
	/// </param>
	/// <param name="args">
	/// The command line arguments.
	/// </param>
	/// <returns>
	/// The initialized IHostBuilder.
	/// </returns>
	public static IHostBuilder CreateDefaultBuilder(Assembly applicationAssembly, string[]? args = null)
	{
		PlatformHelper.SetAppAssembly(applicationAssembly);
		applicationAssembly = PlatformHelper.GetAppAssembly()!;
		return new HostBuilder()
			.ConfigureCustomDefaults(args)
			.ConfigureAppConfiguration((ctx, appConfig) =>
			{
				var dataFolder = ApplicationDataExtensions.DataFolder();

				var appHost = ctx.HostingEnvironment.FromHostEnvironment(dataFolder, applicationAssembly);
				ctx.HostingEnvironment = appHost;
			})
			.ConfigureServices((ctx, services) =>
			{
				if (ctx.HostingEnvironment is IAppHostEnvironment appHost)
				{
					services.AddSingleton(appHost);
				}
				if (ctx.HostingEnvironment is IDataFolderProvider dataProvider)
				{
					services.AddSingleton(dataProvider);
				}
				if (ctx.HostingEnvironment is IHasAddressBar addressBarHost)
				{
					services.AddSingleton(addressBarHost);
				}
			})
				.ConfigureHostConfiguration(config =>
				{
					if (PlatformHelper.IsWebAssembly)
					{
						var href = Imports.GetLocation();
						var appsettingsPrefix = new Dictionary<string, string?>
							{
								{ HostingConstants.AppConfigPrefixKey, "local" },
								{ HostingConstants.LaunchUrlKey, href }
							};
						config.AddInMemoryCollection(appsettingsPrefix);

						var query = new Uri(href).Query;
						var queriesValues = System.Web.HttpUtility.ParseQueryString(query);
						var queryDict = (from k in queriesValues.AllKeys
										 select new { Key = k, Value = queriesValues[k] }).ToDictionary(x => x.Key, x => (string?)x.Value);
						config.AddInMemoryCollection(queryDict);
					}
				})
			.ConfigureServices((ctx, services) => services.Configure<HostConfiguration>(ctx.Configuration.GetSection(nameof(HostConfiguration))))
			.UseStorage();
	}
}
