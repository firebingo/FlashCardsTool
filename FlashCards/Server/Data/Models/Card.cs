using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models
{
	public class Card
	{
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }
		[Column("setId")]
		public long SetId { get; set; }
		[Column("frontValue")]
		public string? FrontValue { get; set; }
		[Column("backValue")]
		public string? BackValue { get; set; }
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; } = DateTime.MinValue;
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;

		public virtual CardSet? CardSet { get; set; }
	}
}
