using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FlashCards.Shared.Util
{
	public static class FileUtil
	{
		public static async Task<string[]> ReadEmbeddedResourceLinesAsync(Assembly assembly, string resource)
		{
			Stream? stream = null;
			try
			{
				stream = assembly.GetManifestResourceStream(resource);
				if (stream == null)
					return Array.Empty<string>();

				var result = new List<string>();
				using StreamReader reader = new StreamReader(stream);
				string? s = null;
				while ((s = await reader.ReadLineAsync()) != null)
				{
					result.Add(s);
				}

				return result.ToArray();
			}
			catch
			{
				return Array.Empty<string>();
			}
			finally
			{
				if (stream != null)
					await stream.DisposeAsync();
			}
		}
	}
}
