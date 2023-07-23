﻿using System.Net;
using Autofac;
using PassWinmenu.Configuration;

namespace PassWinmenu;

internal sealed class Setup
{
	/// <summary>
	/// Loads all required resources.
	/// </summary>
	public static IContainer InitialiseDesktop(ConfigManager configManager)
	{
		// Load compiled-in resources.
		EmbeddedResources.Load();

		Log.Send("------------------------------");
		Log.Send($"Starting pass-winmenu {Program.Version}");
		Log.Send("------------------------------");

		Log.Send($"Enabled security protocols: {ServicePointManager.SecurityProtocol}");

#if DEBUG
		Log.EnableFileLogging();
#else
			if (ConfigManager.Config.CreateLogFile)
			{
				Log.EnableFileLogging();
			}
#endif

		var container = new DependenciesBuilder()
			.RegisterDesktopNotifications()
			.RegisterConfiguration(configManager)
			.RegisterEnvironment()
			.RegisterActions()
			.RegisterGpg()
			.RegisterGit()
			.RegisterApplication()
			.RegisterJobs()
			.Build();

		return container;
	}

	public static IContainer InitialiseCommandLine(ConfigManager configManager)
	{
		// Load compiled-in resources.
		EmbeddedResources.Load();
		
		var container = new DependenciesBuilder()
			.RegisterCommandLineNotifications()
			.RegisterConfiguration(configManager)
			.RegisterEnvironment()
			.RegisterActions()
			.RegisterGpg()
			.RegisterGit()
			.RegisterApplication()
			.Build();

		return container;
	}
}
