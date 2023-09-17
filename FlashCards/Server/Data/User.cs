using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data
{
	public class User
	{
		[Column("id")]
		[Key]
		public long Id { get; set; }
		[Column("email")]
		public string? Email { get; set; } = string.Empty;
		[Column("username")]
		public string UserName { get; set; } = string.Empty;
		[Column("password")]
		public ulong Password { get; set; }
		[Column("requiresNewPassword")]
		public bool RequiresNewPassword { get; set; }
		[Column("salt")]
		public uint Salt { get; set; } = 0;
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; } = DateTime.MinValue;
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;
	}
}
