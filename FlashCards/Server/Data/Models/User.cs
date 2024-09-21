using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models
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
		[Column("disabled")]
		public bool Disabled { get; set; } = false;
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; } = DateTime.MinValue;
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;

		public virtual ICollection<CardSet>? CardSets { get; set; }
		public virtual ICollection<CardSetCollection>? Collections { get; set; }
	}
}
