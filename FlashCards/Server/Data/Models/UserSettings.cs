using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data.Models
{
	public class UserSettings
	{
		[Column("userId")]
		[Key]
		public long UserId { get; set; }
		[Column("colorCardPercent")]
		public bool ColorCardPercent { get; set; }
		[Column("colorCardThreshold")]
		public int ColorCardThreshold { get; set; }
		[Column("showCardColorInGame")]
		public bool ShowCardColorInGame { get; set; }
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; } = DateTime.MinValue;

		public virtual User? User { get; set; }
	}
}
