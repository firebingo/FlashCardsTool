using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models
{
	public class CardSetCollectionSets
	{
		[Column("collectionId")]
		public long CollectionId { get; set; }
		[Column("setId")]
		public long SetId { get; set; }
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; } = DateTime.MinValue;
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;

		public virtual CardSetCollection? Collection { get; set; }
		public virtual CardSet? CardSet { get; set; }
	}
}
