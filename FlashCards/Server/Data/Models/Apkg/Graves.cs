using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models.Apkg
{
	// Contains deleted cards, notes, and decks that need to be synced.
	// usn should be set to -1, 
	// oid is the original id.
	// type: 0 for a card, 1 for a note and 2 for a deck
	[Table("graves")]
	public class Graves
	{
		[Column("usn")]
		public long UpdateSequenceNumber { get; set; }
		[Column("oid")]
		public long OriginalId { get; set; }
		[Column("type")]
		public long Type { get; set; }
	}
}
