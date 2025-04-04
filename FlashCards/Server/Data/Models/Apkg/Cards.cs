using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models.Apkg
{
	// Cards are what you review.
	// There can be multiple cards for each note, as determined by the Template.
	[Table("cards")]
	[Index(nameof(NoteId), Name = "ix_cards_nid")]
	[Index(nameof(DeckId), nameof(Queue), nameof(Due), Name = "ix_cards_sched")]
	[Index(nameof(UpdateSequenceNumber), Name = "ix_cards_usn")]
	public class Cards
	{
		[Key]
		[Column("id")]
		public long Id { get; set; } // the epoch milliseconds of when the card was created
		[Column("nid")]
		public long NoteId { get; set; } // notes.id
		[Column("did")]
		public long DeckId { get; set; } // deck id (available in col table)
		[Column("ord")]
		public long Ordinal { get; set; } // ordinal : identifies which of the card templates or cloze deletions it corresponds to 
										  // for card templates, valid values are from 0 to num templates - 1
										  // for cloze deletions, valid values are from 0 to max cloze index - 1 (they're 0 indexed despite the first being called `c1`)
		[Column("mod")]
		public long ModifiedTime { get; set; } // modification time as epoch seconds
		[Column("usn")]
		public long UpdateSequenceNumber { get; set; } // update sequence number : used to figure out diffs when syncing. 
													   // value of -1 indicates changes that need to be pushed to server.
													   // usn<server usn indicates changes that need to be pulled from server.
		[Column("type")]
		public long Type { get; set; } // 0=new, 1=learning, 2=review, 3=relearning
		[Column("queue")]
		public long Queue { get; set; } // -3=user buried(In scheduler 2),
										// -2=sched buried(In scheduler 2), 
										// -2=buried(In scheduler 1),
										// -1=suspended,
										// 0=new, 1=learning, 2=review(as for type)
										// 3=in learning, next rev in at least a day after the previous review
										// 4=preview
		[Column("due")]
		public long Due { get; set; } // Due is used differently for different card types: 
									  // new: the order in which cards are to be studied; starts from 1.
									  // learning/relearning: epoch timestamp in seconds
									  // review: days since the collection's creation time
		[Column("ivl")]
		public long Interval { get; set; } // interval (used in SRS algorithm). Negative = seconds, positive = days
										   // v2 scheduler used seconds for (re) learning cards and days for review cards
										   // v3 scheduler uses seconds only for intraday(re)learning cards and days for interday(re)learning cards and review cards
		[Column("factor")]
		public long Factor { get; set; } // The ease factor of the card in permille (parts per thousand). If the ease factor is 2500, the card’s interval will be multiplied by 2.5 the next time you press Good.
		[Column("reps")]
		public long Reps { get; set; } // number of reviews
		[Column("lapses")]
		public long Lapses { get; set; } // the number of times the card went from a "was answered correctly" 
										 // to "was answered incorrectly" state
		[Column("left")]
		public long Left { get; set; } // of the form a*1000+b, with:
									   // a the number of reps left today
									   // b the number of reps left till graduation
									   // for example: '2004' means 2 reps left today and 4 reps till graduation
		[Column("odue")]
		public long OriginalDue { get; set; } // original due: In filtered decks, it's the original due date that the card had before moving to filtered.
											  // If the card lapsed in scheduler1, then it's the value before the lapse. (This is used when switching to scheduler 2. At this time, cards in learning becomes due again, with their previous due date)
											  // In any other case it's 0.
		[Column("odid")]
		public long OriginalDeckId { get; set; } // original did: only used when the card is currently in filtered deck
		[Column("flags")]
		public long Flags { get; set; } // an integer. This integer mod 8 represents a "flag", which can be see in browser and while reviewing a note. Red 1, Orange 2, Green 3, Blue 4, no flag: 0. This integer divided by 8 represents currently nothing
		[Column("data", TypeName = "text")]
		public string Data { get; set; } = string.Empty; // currently unused
	}
}
