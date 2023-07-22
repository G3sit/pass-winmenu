using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PassWinmenu.Configuration;
using PassWinmenu.PasswordManagement;
using PassWinmenu.Utilities;
using PassWinmenu.WinApi;

#nullable enable
namespace PassWinmenu.Windows
{
	internal class DialogCreator
	{
		private readonly INotificationService notificationService;
		private readonly IPasswordManager passwordManager;
		private readonly PathDisplayService pathDisplayService;
		private readonly Config config;

		public DialogCreator(
			INotificationService notificationService,
			IPasswordManager passwordManager,
			PathDisplayService pathDisplayService,
			Config config)
		{
			this.notificationService = notificationService;
			this.passwordManager = passwordManager;
			this.pathDisplayService = pathDisplayService;
			this.config = config;
		}

		/// <summary>
		/// Opens a window where the user can choose the location for a new password file.
		/// </summary>
		/// <returns>The path to the file that the user has chosen</returns>
		public string? ShowFileSelectionWindow()
		{
			SelectionWindowConfiguration windowConfig;
			try
			{
				windowConfig = SelectionWindowConfiguration.ParseMainWindowConfiguration(config);
			}
			catch (ConfigurationParseException e)
			{
				notificationService.Raise(e.Message, Severity.Error);
				return null;
			}

			// Ask the user where the password file should be placed.
			var pathWindow = new FileSelectionWindow(passwordManager.PasswordStore, windowConfig, config.Interface, "Choose a location...");
			pathWindow.ShowDialog();
			if (!pathWindow.Success)
			{
				return null;
			}
			return pathWindow.SelectionText + Program.EncryptedFileExtension;
		}

		/// <summary>
		/// Asks the user to choose a password file.
		/// </summary>
		/// <returns>
		/// The path to the chosen password file (relative to the password directory),
		/// or null if the user didn't choose anything.
		/// </returns>
		public PasswordFile? RequestPasswordFile()
		{
			Helpers.AssertOnUiThread();

			// Find GPG-encrypted password files
			var passFiles = passwordManager.GetPasswordFiles().ToList();
			if (passFiles.Count == 0)
			{
				MessageBox.Show("Your password store doesn't appear to contain any passwords yet.", "Empty password store", MessageBoxButton.OK, MessageBoxImage.Information);
				return null;
			}
			return ShowPasswordMenu(passFiles, pathDisplayService.GetDisplayPath, "Select a password...").ValueOrDefault();
		}



		/// <summary>
		/// Opens the password menu and displays it to the user, allowing them to choose an existing password file.
		/// </summary>
		/// <param name="options">A list of options the user can choose from.</param>
		/// <param name="keySelector">A function that selects a string to display for each option in <paramref name="options"/>.</param>
		/// <param name="hint">A search hint to show, or null if no hint should be shown.</param>
		/// <returns>One of the values contained in <paramref name="options"/>, or the default value of <typeparamref name="TEntry"/> if no option was chosen.</returns>
		public Option<TEntry> ShowPasswordMenu<TEntry>(IEnumerable<TEntry> options, Func<TEntry, string> keySelector, string hint)
		{
			SelectionWindowConfiguration windowConfig;
			try
			{
				windowConfig = SelectionWindowConfiguration.ParseMainWindowConfiguration(config);
			}
			catch (ConfigurationParseException e)
			{
				notificationService.Raise(e.Message, Severity.Error);
				return default;
			}

			var menu = new PasswordSelectionWindow<TEntry>(options, keySelector, windowConfig, config.Interface, hint);
			menu.ShowDialog();
			if (menu.Success)
			{
				return Option.Some(menu.Selection);
			}

			return default;
		}

	}
}
