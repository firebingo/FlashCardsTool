namespace FlashCards.Server.Models
{
	public class IndexResult
	{
		public string Text { get; set; } = string.Empty;
		public string Text2 { get; set; } = string.Empty;
		public bool IsCard { get; set; }
		public long? CardId { get; set; }
		public bool IsSet { get; set; }
		public long? SetId { get; set; }
		public bool IsCollection { get; set; }
		public long? CollectionId { get; set; }
	}
}
