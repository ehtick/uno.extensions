﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using ExtensionsSampleApp.Navigators;
using ExtensionsSampleApp.UnoCommerce;
using ExtensionsSampleApp.UnoCommerce.Services;
using ExtensionsSampleApp.UnoCommerce.ViewModels;
using ExtensionsSampleApp.ViewModels;
using ExtensionsSampleApp.ViewModels.Twitter;
using ExtensionsSampleApp.Views;
using ExtensionsSampleApp.Views.Twitter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Uno.Extensions;
using Uno.Extensions.Configuration;
using Uno.Extensions.Hosting;
using Uno.Extensions.Logging;
using Uno.Extensions.Logging.Serilog;
using Uno.Extensions.Navigation;
using Uno.Extensions.Navigation.Controls;
using Uno.Extensions.Navigation.Regions;
#if !WINDOWS_UWP
using Uno.Foundation;
#endif
using Uno.UI.ToolkitLib;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

#if WINDOWS_UWP || UNO_UWP_COMPATIBILITY
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using ExtensionsSampleApp.Controls;
#else
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
//using Microsoft.UI.Xaml.Navigation;
#endif

namespace ExtensionsSampleApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application, IRecipient<RegionUpdatedMessage>
    {
#if NET5_0 && WINDOWS
        private Window _window;

#else
        private Windows.UI.Xaml.Window _window;
#endif
        private IHost Host { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Host = UnoHost
               .CreateDefaultBuilder(true)
               .UseEnvironment(Environments.Development)
               //.UseLogging()
               //.ConfigureLogging(logBuilder =>
               //{
               //    logBuilder
               //         .SetMinimumLevel(LogLevel.Trace)
               //         .XamlLogLevel(LogLevel.Information)
               //         .XamlLayoutLogLevel(LogLevel.Information);
               //})
               //.UseSerilog(true, true)
               //.UseEmbeddedAppSettings<App>() 
               .UseConfigurationSectionInApp<CommerceSettings>()
               .UseSettings<CommerceSettings>()
               .ConfigureServices(services =>
               {
                   services
                                       .AddRegion<TabView, TabViewNavigator>()
                                      .AddRegion<Control, ControlVisualStateNavigator>(ControlVisualStateNavigator.NavigatorName)
#if __IOS__
                                       .AddRegion<Picker, PickerNavigator>()
#endif
                   .AddRegion<TabBar, TabBarRegion>()
                    .AddSingleton<INavigationBindingHandler, TabBarItemNavigationBindingHandler>()
                    .AddSingleton<INavigationBindingHandler, NavigationViewItemNavigationBindingHandler>()

                   .AddTransient<MainViewModel>()
                   .AddTransient<SecondViewModel>()
                   .AddViewModelData<Widget>()
                   .AddTransient<ThirdViewModel>()
                   .AddTransient<FourthViewModel>()
                   .AddTransient<TabbedViewModel>()
                   .AddTransient<TabDoc0ViewModel>()
                   .AddTransient<TabDoc1ViewModel>()
                   .AddTransient<TweetsViewModel>()
                   .AddTransient<TweetDetailsViewModel>()
                   .AddTransient<NotificationsViewModel>()
                   .AddTransient<Content2ViewModel>()
                   .AddViewModelData<Tweet>()
                   .AddTransient<ProductsViewModel>()
                   .AddTransient<FilterViewModel>()
                   .AddTransient<ProductDetailsViewModel>()
                   .AddViewModelData<Product>()
                   .AddTransient<DealsViewModel>()
                   .AddSingleton<IProductService, ProductService>();
               })
               /*
                * .ConfigureNavigationMapping(mapping=>{
                *   mapping
                *      .Register("path")                    // Path = path
                *      .ForView<TView>()                    // View = typeof(TView)
                *      .WithViewModel<TViewModel>()         // ViewModel = typeof(TViewModel) <-- also need to register with DI
                *      .HandlesData<TData>()                // Data = typeof(TData)  <-- also need to register with DI 
                * })
                */
               .UseNavigation()
               .Build()
               .EnableUnoLogging();
            var logger = Host.Services.GetService<ILogger<App>>();
            var env = Host.Services.GetService<IAppHostEnvironment>();
            logger.LogInformation("Host:" + env.ContentRootPath + " - " + env.AppDataPath);
            logger.LogTrace("App Trace");
            logger.LogDebug("App Debug");
            logger.LogInformation("App Info");
            logger.LogWarning("App Warning");
            logger.LogError("App Error");
            logger.LogCritical("App Critical");
            var mapping = Host.Services.GetService<IRouteMappings>();


            mapping.Register(new RouteMap(typeof(MainPage).Name, typeof(MainPage), typeof(MainViewModel)));
            mapping.Register(new RouteMap(typeof(SecondPage).Name, typeof(SecondPage), typeof(SecondViewModel), typeof(Widget), typeof(Widget)));
            //mapping.Register(new RouteMap(typeof(ThirdPage).Name, typeof(ThirdPage), typeof(ThirdViewModel)));
            //mapping.Register(new RouteMap(typeof(FourthPage).Name, typeof(FourthPage), typeof(FourthViewModel)));
            mapping.Register(new RouteMap(typeof(TabbedPage).Name, typeof(TabbedPage), typeof(TabbedViewModel),
                RegionInitialization: (region, nav) => nav.Route.IsEmpty() ?
                                        nav with { Route = nav.Route with { Base = "doc2" } } :
                                        nav));
            mapping.Register(new RouteMap(typeof(CommerceHomePage).Name, typeof(CommerceHomePage),
                RegionInitialization: (region, nav) => nav.Route.Next().IsEmpty() ?
                                        nav with { Route = nav.Route.Append(Route.NestedRoute("Products")) } :
                                        nav));
            mapping.Register(new RouteMap("Products", typeof(FrameView),
                RegionInitialization: (region, nav) => nav.Route.Next().IsEmpty() ?
                                        nav with { Route = nav.Route.AppendPage<ProductsPage>() } :                                        nav with
                                        {
                                            Route = nav.Route.ContainsView<ProductsPage>() ?
                                                            nav.Route :
                                                            nav.Route.InsertPage<ProductsPage>()
                                        }));
            //mapping.Register(new RouteMap(typeof(ProductsPage).Name, typeof(ProductsPage), typeof(ProductsViewModel));
            mapping.Register(new RouteMap("Deals", typeof(FrameView),
                RegionInitialization: (region, nav) => nav.Route.IsEmpty() ?
                                        nav with { Route = nav.Route with { Base = "+DealsPage/HotDeals" } } :
                                        nav with { Route = nav.Route with { Path = "+DealsPage/HotDeals" } }));
            mapping.Register(new RouteMap("ProductDetails",
                typeof(ProductDetailsPage),
                typeof(ProductDetailsViewModel), typeof(Product),
                BuildQueryParameters: entity => new Dictionary<string, string> { { "ProductId", (entity as Product)?.ProductId + "" } }));
            mapping.Register(new RouteMap(typeof(TabBarPage).Name, typeof(TabBarPage)));
            mapping.Register(new RouteMap("doc0", ViewModel: typeof(TabDoc0ViewModel)));
            mapping.Register(new RouteMap("doc1", ViewModel: typeof(TabDoc1ViewModel)));
            mapping.Register(new RouteMap(typeof(Content1).Name, typeof(Content1)));
            mapping.Register(new RouteMap(typeof(Content2).Name, typeof(Content2), typeof(Content2ViewModel)));
            mapping.Register(new RouteMap(typeof(SimpleContentDialog).Name, typeof(SimpleContentDialog)));
            mapping.Register(new RouteMap(typeof(TwitterPage).Name, typeof(TwitterPage),
                RegionInitialization: (region, nav) => nav.Route.IsEmpty() ?
                                        nav with { Route = nav.Route with { Base = "home" } } :
                                        nav));
            mapping.Register(new RouteMap("home",
                RegionInitialization: (region, nav) => nav.Route.Next().IsEmpty() ?
                                          nav with { Route = nav.Route.AppendPage<TweetsPage>() } : nav with
                                          {
                                              Route = nav.Route.ContainsView<TweetsPage>() ?
                                                              nav.Route :
                                                              nav.Route.InsertPage<TweetsPage>()
                                          }));
            //(region, nav) => string.IsNullOrWhiteSpace(nav.Route.Path) ?
            //                        nav with { Route = nav.Route with { Path = typeof(TweetsPage).Name } } :
            //                        nav with
            //                        {
            //                            Route = nav.Route with
            //                            {
            //                                Path = nav.Route.Path
            //                                          .TrimStart('+')
            //                                          .StartsWith(typeof(TweetsPage).Name) ?
            //                                       nav.Route.Path :
            //                                       typeof(TweetsPage).Name + Schemes.NavigateForward + nav.Route.Path
            //                            }
            //                        }
            //                        ));
            mapping.Register(new RouteMap("notifications",
                RegionInitialization: (region, nav) => nav.Route.Next().IsEmpty() ?
                                         nav with { Route = nav.Route.AppendPage<NotificationsPage>() } : nav with
                                         {
                                             Route = nav.Route.ContainsView<NotificationsPage>() ?
                                                             nav.Route :
                                                             nav.Route.InsertPage<NotificationsPage>()
                                         }));
            //(region, nav) => string.IsNullOrWhiteSpace(nav.Route.Path) ?
            //                        nav with { Route = nav.Route with { Path = typeof(NotificationsPage).Name } } :
            //                        nav with
            //                        {
            //                            Route = nav.Route with
            //                            {
            //                                Path = nav.Route.Path
            //                                          .TrimStart('+')
            //                                          .StartsWith(typeof(NotificationsPage).Name) ?
            //                                       nav.Route.Path :
            //                                       typeof(NotificationsPage).Name + Schemes.NavigateForward + nav.Route.Path
            //                            }
            //                        }
            //                        ));
            mapping.Register(new RouteMap(typeof(TweetsPage).Name, typeof(TweetsPage), typeof(TweetsViewModel)));
            mapping.Register(new RouteMap(typeof(TweetDetailsPage).Name, typeof(TweetDetailsPage),
                typeof(TweetDetailsViewModel), typeof(Tweet),
                BuildQueryParameters: entity => new Dictionary<string, string> { { "TweetId", (entity as Tweet)?.Id + "" } }));
            mapping.Register(new RouteMap(typeof(NotificationsPage).Name, typeof(NotificationsPage), typeof(NotificationsViewModel)));
            mapping.Register(new RouteMap(typeof(TwitterProfilePage).Name, typeof(TwitterProfilePage)));
            mapping.Register(new RouteMap(typeof(NavigationViewPage).Name, typeof(NavigationViewPage)));
            mapping.Register(new RouteMap(typeof(NavigationViewGridVisibilityPage).Name, typeof(NavigationViewGridVisibilityPage)));
            mapping.Register(new RouteMap(typeof(NavigationViewVisualStatesPage).Name, typeof(NavigationViewVisualStatesPage)));
            mapping.Register(new RouteMap(typeof(CustomFlyout).Name, typeof(CustomFlyout), typeof(CustomViewModel)));

            var notifier = Host.Services.GetService<INavigationNotifier>();
            notifier.Register(this);

            //InitializeLogging();

            this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
            this.Suspending += OnSuspending;
#endif
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

#if NET5_0 && WINDOWS
            _window = new Window();
            _window.Activate();
#else
            _window = Windows.UI.Xaml.Window.Current;
#endif

            var rootFrame = _window.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame().WithNavigation(Host.Services);
                rootFrame.NavigationFailed += OnNavigationFailed;


#if NET5_0

                var backnav = Host.Services.GetService<INavigator>();
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += (s, a) =>
                {
                    var appTitle = ApplicationView.GetForCurrentView();
                    appTitle.Title = "Back pressed - " + DateTime.Now.ToString("HH:mm:ss");
                    backnav.GoBack(this);
                };
#endif

                //navResult.OnCompleted(() => Debug.WriteLine("Nav complete"));
                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                // Place the frame in the current Window
                _window.Content = rootFrame;
            }

#if !(NET5_0 && WINDOWS)
            if (e.PrelaunchActivated == false)
#endif
            {
                if (rootFrame.Content == null)
                {
                    //// When the navigation stack isn't restored navigate to the first page,
                    //// configuring the new page by passing required information as a navigation
                    //// parameter
                    //rootFrame.Navigate(typeof(TabbedPage), e.Arguments);
                }
                // Ensure the current window is active
                _window.Activate();
            }

            await Task.Run(async () =>
            {
                //Startup.Start();
                //Host.Run();
                await Host.StartAsync();
            });

            //            var logger = Host.Services.GetService<ILogger<App>>();
            //            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(
            //new Uri("ms-appx:///appsettings.json"));
            //            logger.LogInformation("Setting Path:" + storageFile.Path);
            //            var settings = File.ReadAllText(storageFile.Path);
            //            logger.LogInformation("Setting:" + settings);

            var nav2 = Host.Services.GetService<INavigator>();

#if NET5_0
                var href = WebAssemblyRuntime.InvokeJS("window.location.href");
                var url = new UriBuilder(href);
                var query = url.Query;
                var path = (url.Path + (!string.IsNullOrWhiteSpace(query)?"?":"") + query +"").TrimStart('/');
                if(!string.IsNullOrWhiteSpace(path))
                {
                    var navResult = nav.NavigateToRouteAsync(this, path, Schemes.Root);
                }
                else
                {
                    var navResult = nav.NavigateToRouteAsync(this, "+MainPage" + path, Schemes.Root);

                }
#else
            var nav = Host.Services.GetService<INavigator>();
            //var navResult = nav.NavigateToViewAsync<MainPage>(this, Schemes.Nested);
            //var navResult = nav.NavigateToViewAsync<MainPage>(this, Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "+MainPage", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "+MainPage+SecondPage", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "+MainPage+SecondPage+ThirdPage", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "+MainPage+SecondPage/content/Content1/", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "/MainPage/SecondPage/content/Content1/", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "TabbedPage/doc1", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "TabbedPage/doc2/SecondPage/content/Content1", Schemes.Root);
            //var navResult = nav.NavigateToRouteAsync(this, "TwitterPage/notifications/TweetDetailsPage?TweetId=23", Schemes.Root);
            var navResult = nav.NavigateToViewAsync<LoginPage>(this);
            //var navResult = nav.NavigateToRouteAsync(this, "/CommerceHomePage/Products/ProductDetails?ProductId=3");

#endif

        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
                builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
                builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
                builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;
        }

        public void Receive(RegionUpdatedMessage message)
        {
            var rootRegion = message.Region.Root();
            var route = (rootRegion.GetRoute() + "").Replace("+", "/");
            var appTitle = ApplicationView.GetForCurrentView();
            appTitle.Title = "Navigation: " + route;

#if NET5_0
            WebAssemblyRuntime.InvokeJS($"window.history.pushState(\"{ route}\",\"ExtensionsApp\", \"{ route}\");");
#endif
        }

    }

    public static class VisualTreeUtils
    {
        public static T FindVisualChildByType<T>(this DependencyObject element)
            where T : DependencyObject
        {
            if (element == null)
            {
                return default(T);
            }

            if (element is T elementAsT)
            {
                return elementAsT;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                var result = VisualTreeHelper.GetChild(element, i).FindVisualChildByType<T>();
                if (result != null)
                {
                    return result;
                }
            }

            return default(T);
        }

        public static FrameworkElement FindVisualChildByName(this DependencyObject element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            if (element is FrameworkElement elementAsFE && elementAsFE.Name == name)
            {
                return elementAsFE;
            }

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                var result = VisualTreeHelper.GetChild(element, i).FindVisualChildByName(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}