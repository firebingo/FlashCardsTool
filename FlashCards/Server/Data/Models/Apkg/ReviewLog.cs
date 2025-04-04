using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models.Apkg
{
	// revlog is a review history; it has a row for every review you've ever done!
	[Table("revlog")]
	[Index(nameof(CardId), Name = "ix_revlog_cid")]
	[Index(nameof(UpdateSequenceNumber), Name = "ix_revlog_usn")]
	public class ReviewLog
	{
		[Key]
		[Column("id")]
		public long Id { get; set; } // epoch-milliseconds timestamp of when you did the review
		[Column("cid")]
		public long CardId { get; set; } // cards.id
		[Column("usn")]
		public long UpdateSequenceNumber { get; set; } // update sequence number: for finding diffs when syncing.
													   // See the description in the cards table for more info
		[Column("ease")]
		public long Ease { get; set; } // which button you pushed to score your recall.
									   // review:  1(wrong), 2(hard), 3(ok), 4(easy)
									   // learn/relearn:   1(wrong), 2(ok), 3(easy)
		[Column("ivl")]
		public long Interval { get; set; } // interval (i.e. as in the card table)
		[Column("lastIvl")]
		public long LastInterval { get; set; } // last interval (i.e. the last value of ivl. Note that this value is not necessarily equal to the actual interval between this review and the preceding review)
		[Column("factor")]
		public long Factor { get; set; }
		[Column("time")]
		public long Time { get; set; } // how many milliseconds your review took, up to 60000 (60s)
		[Column("type")]
		public long Type { get; set; } // 0=learn, 1=review, 2=relearn, 3=filtered, 4=manual, 5=rescheduled
	}
}
