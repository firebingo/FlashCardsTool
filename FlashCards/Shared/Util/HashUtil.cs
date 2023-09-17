using System.Security.Cryptography;
using System.Text;

namespace FlashCards.Shared.Util
{
	public static class HashUtil
	{
		public static ulong HashPassword(string password, uint salt) => Sha512HashString($"{password}{salt}");

		public static ulong Sha512HashString(string input)
		{
			var bytes = Encoding.UTF8.GetBytes(input);
			var hash = SHA512.HashData(bytes);
			return BitConverter.ToUInt64(hash);
		}

		public static uint GenerateSalt() => BitConverter.ToUInt32(RandomNumberGenerator.GetBytes(32));
	}
}
