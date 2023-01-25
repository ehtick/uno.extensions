﻿using Uno.Extensions.Reactive;
using Uno.Extensions.Toolkit;

namespace Playground.ViewModels;

[ReactiveBindable(false)]
public partial class ThemeSwitchViewModel:ObservableObject
{
	private readonly IThemeService _ts;

	[ObservableProperty]
	private string isDarkTheme;

	public ThemeSwitchViewModel(IThemeService themeService,IDispatcher dispatcher)
	{
		_ts = themeService;
		_ts.DesiredThemeChanged += ts_DesiredThemeChanged;
		_ = dispatcher.ExecuteAsync(() =>
		{
			IsDarkTheme = themeService.IsDark.ToString();
		});
	}

	private void ts_DesiredThemeChanged(object? sender, AppTheme e)
	{
		IsDarkTheme = _ts.IsDark.ToString();
		Console.WriteLine($"Theme was changed to:{e.ToString()}");
		Console.WriteLine($"Desired Theme is:{_ts.Theme}");
	}

	public async Task ChangeToSystem()
	{
		await _ts.SetThemeAsync(AppTheme.System);
	}

	public async Task ChangeToLight()
	{
		await _ts.SetThemeAsync(AppTheme.Light);
	}

	public async Task ChangeToDark()
	{
		await _ts.SetThemeAsync(AppTheme.Dark);
	}
}

