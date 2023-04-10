using System;
using System.Drawing;
using System.IO;
using System.Reflection;

#nullable enable
namespace PassWinmenu
{
	public static class EmbeddedResources
	{
		public static Icon Icon => new Icon(LoadResource("PassWinmenu.embedded.pass-winmenu-plain.ico"));
		public static Icon IconAhead => new Icon(LoadResource("PassWinmenu.embedded.pass-winmenu-ahead.ico"));
		public static Icon IconBehind => new Icon(LoadResource("PassWinmenu.embedded.pass-winmenu-behind.ico"));
		public static Icon IconDiverged => new Icon(LoadResource("PassWinmenu.embedded.pass-winmenu-diverged.ico"));
		public static Stream DefaultConfig => LoadResource("PassWinmenu.embedded.default-config.yaml");
		public static string Version { get; private set; } = UnknownVersion;

		public const string UnknownVersion = "<unknown version>";

		public static void Load()
		{
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PassWinmenu.embedded.version.txt");
			if (stream == null)
			{
				throw new InvalidOperationException("Version number could not be read from the assembly.");
			}
			using var reader = new StreamReader(stream);
			Version = reader.ReadLine();
		}

		private static Stream LoadResource(string name)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(name)
				?? throw new Exception($"Unable to find required resource '{name}'");
		}
	}
}
