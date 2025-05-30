﻿using System.Reflection;

namespace TestHarness.Ext.Navigation.PageNavigation;

public class PageNavigationRegisterHostInit: PageNavigationHostInit
{
	protected override void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
	{
		views.Register(
			new ViewMap<PageNavigationOnePage, PageNavigationOneViewModel>(),
			new DataViewMap<PageNavigationTwoPage, PageNavigationTwoViewModel, PageNavigationModel>(
				ToQuery: model => new Dictionary<string, string>() { { nameof(PageNavigationModel.Value), model.Value } },

				FromQuery: (sp, query) => Task.FromResult<PageNavigationModel?>(
					query.TryGetValue(nameof(PageNavigationModel.Value), out object? value) && value is string id
					? new(id)
					: null
				)
			),
			new DataViewMap<PageNavigationThreePage, PageNavigationThreeViewModel, PageNavigationModel>(
				ToQuery: model => new Dictionary<string, string>() { { nameof(PageNavigationModel.Value), model.Value } },

				FromQuery: (sp, query) => Task.FromResult<PageNavigationModel?>(
					query.TryGetValue(nameof(PageNavigationModel.Value), out object? value) && value is string id
					? new(id)
					: null
				)),
			new ViewMap<PageNavigationFourPage, PageNavigationFourViewModel>(),
			new ViewMap<PageNavigationFivePage, PageNavigationFiveViewModel>(),
			new ViewMap<PageNavigationSixPage, PageNavigationSixViewModel>(),
			new DataViewMap<PageNavigationSevenPage, PageNavigationSevenViewModel, Widget>(),
			new ViewMap<PageNavigationEightPage, PageNavigationEightViewModel>(),
			new ViewMap<PageNavigationNinePage, PageNavigationNineViewModel>(),
			new ViewMap<PageNavigationTenPage, PageNavigationTenViewModel>(),
			ConfirmDialog
			);


		// RouteMap required for Shell if initialRoute or initialViewModel isn't specified when calling NavigationHost
		routes.Register(
					new RouteMap("PageNavigationTwo", View: views.FindByViewModel<PageNavigationTwoViewModel>(), IsDefault: true, DependsOn: "PageNavigationOne"),
					new RouteMap("PageNavigationOne", View: views.FindByViewModel<PageNavigationOneViewModel>()),
					new RouteMap("PageNavigationThree", View: views.FindByViewModel<PageNavigationThreeViewModel>(), DependsOn: "PageNavigationTwo"),
					new RouteMap("PageNavigationFour", View: views.FindByViewModel<PageNavigationFourViewModel>()),
					new RouteMap("PageNavigationFive", View: views.FindByViewModel<PageNavigationFiveViewModel>()),
					new RouteMap("PageNavigationSix", View: views.FindByViewModel<PageNavigationSixViewModel>()),
					new RouteMap("PageNavigationSeven", View: views.FindByViewModel<PageNavigationSevenViewModel>(), DependsOn: "PageNavigationSix"),
					new RouteMap("PageNavigationEight", View: views.FindByViewModel<PageNavigationEightViewModel>(), DependsOn: "PageNavigationSeven"),
					new RouteMap("PageNavigationNine", View: views.FindByViewModel<PageNavigationNineViewModel>(), DependsOn: "PageNavigationEight"),
					new RouteMap("PageNavigationTen", View: views.FindByViewModel<PageNavigationTenViewModel>(), DependsOn: "PageNavigationNine"),
					new RouteMap("Confirm", View: ConfirmDialog)
			);
	}

}
