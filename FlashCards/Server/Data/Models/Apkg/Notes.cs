using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models.Apkg
{
	// Notes contain the raw information that is formatted into a number of cards
	// according to the models
	[Table("notes")]
	[Index(nameof(Checksum), Name = "ix_notes_csum")]
	[Index(nameof(UpdateSequenceNumber), Name = "ix_notes_usn")]
	public class Notes
	{
		[Key]
		[Column("id")]
		public long Id { get; set; } // epoch milliseconds of when the note was created
		[Column("guid", TypeName = "text")]
		public string Guid { get; set; } = string.Empty; // globally unique id, almost certainly used for syncing
		[Column("mid")]
		public long ModelId { get; set; }
		[Column("mod")]
		public long ModifiedTime { get; set; } // modification timestamp, epoch seconds
		[Column("usn")]
		public long UpdateSequenceNumber { get; set; } // update sequence number: for finding diffs when syncing.
													   // See the description in the cards table for more info
		[Column("tags", TypeName = "text")]
		public string Tags { get; set; } = string.Empty; // space-separated string of tags. 
														 // includes space at the beginning and end, for LIKE "% tag %" queries
		[Column("flds", TypeName = "text")]
		public string Fields { get; set; } = string.Empty; // the values of the fields in this note. separated by 0x1f (31) character.
		[Column("sfld")]
		public long SortField { get; set; } // sort field: used for quick sorting and duplicate check. The sort field is an integer so that when users are sorting on a field that contains only numbers, they are sorted in numeric instead of lexical order. Text is stored in this integer field.
		[Column("csum")]
		public long Checksum { get; set; } // field checksum used for duplicate check.
										   // integer representation of first 8 digits of sha1 hash of the first field
		[Column("flags")]
		public long Flags { get; set; } // unused
		[Column("data", TypeName = "text")]
		public string Data { get; set; } = string.Empty; // unused
	}
}
