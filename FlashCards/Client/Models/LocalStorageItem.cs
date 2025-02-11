namespace FlashCards.Client.Models
{
	public class LocalStorageItem<T>
	{
		public T? Value { get; set; } = default;
		public long ExpireTimeUnix { get; set; }
	}
}
