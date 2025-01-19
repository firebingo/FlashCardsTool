using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models
{
	public class PlayStats
	{
		[Column("id")]
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long Id { get; set; }
		[Column("userId")]
		public long UserId { get; set; }
		[Column("setId")]
		public long? SetId { get; set; }
		[Column("collectionId")]
		public long? CollectionId { get; set; }
		[Column("passCount")]
		public int PassCount { get; set; }
		[Column("missCount")]
		public int MissCount { get; set; }
		[Column("playTime")]
		public TimeSpan PlayTime { get; set; }
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; } = DateTime.MinValue;

		public virtual User? User { get; set; }
		public virtual CardSet? CardSet { get; set; }
		public virtual CardSetCollection? Collection { get; set; }
	}
}
