using System.Text.RegularExpressions;

namespace FlashCards.Shared.Util
{
	public static partial class RegexUtil
	{
		[GeneratedRegex(@"^.*@.*\..{0,63}$")]
		public static partial Regex SimpleEmailRegex();
	}
}
