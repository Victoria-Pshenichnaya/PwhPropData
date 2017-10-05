using System;
using System.Collections.Generic;

namespace PwhPropData.UI
{
	public class HoldingsStatementViewModel
	{
		public DateTime Date { get; set; }

		public IEnumerable<HoldingDataViewModel> Holdings { get; set; }
	}
}
