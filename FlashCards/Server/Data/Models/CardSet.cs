using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models
{
	public class CardSet
	{
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }
		[Column("userId")]
		public long UserId { get; set; }
		[Column("setName")]
		public string SetName { get; set; } = string.Empty;
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; } = DateTime.MinValue;
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;

		public virtual User? User { get; set; }
		public virtual ICollection<Card>? Cards { get; set; }
	}
}
