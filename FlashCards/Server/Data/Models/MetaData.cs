using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlashCards.Server.Data
{
	public class MetaData
	{
		[Column("mkey")]
		[Key]
		public int Key { get; set; }
		[Column("versionDate")]
		public DateTime VersionDate { get; set; }
		[Column("version")]
		public uint Version { get; set; }
		[Column("createdTime")]
		public DateTime CreatedTime { get; set; }
		[Column("modifiedTime")]
		public DateTime ModifiedTime { get; set; }
	}
}
