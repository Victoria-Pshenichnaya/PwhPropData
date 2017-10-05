using System;
using System.Collections.Generic;

namespace PwhPropData.Core.Entities
{
	public class HoldingsStatement
	{
		public DateTime HoldingsStatementDate { get; set; }
		public DateTime? LastModifiedDateTime { get; set; }
		public IEnumerable<HoldingData> Holdings { get; set; }
	}
}
