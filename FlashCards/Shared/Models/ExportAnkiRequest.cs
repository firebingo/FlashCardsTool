using System.Collections.Generic;

namespace FlashCards.Shared.Models
{
	public class ExportAnkiRequest
	{
		public long UserId { get; set; }
		public List<long> SetIds { get; set; } = [];
	}
}
