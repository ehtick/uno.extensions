﻿namespace TestHarness.Ext.Navigation.Apps.Regions;

public partial class RegionsHostInit : BaseHostInitialization
{
	protected override void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
	{
		views.Register(
			new ViewMap(ViewModel: typeof(RegionsShellModel)),
			new ViewMap<RegionsHomePage>(),
			new ViewMap<RegionsFirstPage>(),
			new ViewMap<RegionsSecondPage, RegionsSecondViewModel>(),
			new ViewMap<RegionsThirdPage>(),
			new DataViewMap<RegionsFourthPage, RegionsFourthViewModel, RegionEntityData>(),
			new ViewMap<RegionsTbDataPage, RegionsTbDataPageViewModel>(),
			new DataViewMap<RegionsFirstTbiDataPage, RegionsFirstTbiDataViewModel, RegionEntityData>(),
			new DataViewMap<RegionsSecondTbiDataPage, RegionsSecondTbiDataViewModel, RegionEntityData>()
		);

		routes.Register(
			new RouteMap("", View: views.FindByViewModel<RegionsShellModel>(),
				Nested:
				[
					new ("RegionsHome", View: views.FindByView<RegionsHomePage>(),
						Nested:
						[
							new ("RegionsFirst", View: views.FindByView<RegionsFirstPage>(), IsDefault: true,
								Nested:
								[
									new ("RegionsOne", IsDefault: true),
									new ("RegionsTwo"),
									new ("RegionsThree")
								]
							),
							new ("RegionsSecond", View: views.FindByViewModel<RegionsSecondViewModel>()),
							new ("RegionsThird", View: views.FindByView<RegionsThirdPage>()),
							new ("RegionsFourth", View: views.FindByViewModel<RegionsFourthViewModel>(), DependsOn: "RegionsSecond"),
							new ("RegionsTbData", View: views.FindByViewModel<RegionsTbDataPageViewModel>(),
								Nested:
								[
									new ("RegionsTbiDataOne", View: views.FindByViewModel<RegionsFirstTbiDataViewModel>()),
									new ("RegionsTbiDataTwo", View: views.FindByViewModel<RegionsSecondTbiDataViewModel>())
								]
							),
						]
					),
				]
			)
		);
	}
}
