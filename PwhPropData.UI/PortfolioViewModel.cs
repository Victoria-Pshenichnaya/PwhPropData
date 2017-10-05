using Microsoft.Practices.Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PwhPropData.UI
{
	public class PortfolioViewModel : BindableBase
	{
		private ObservableCollection<HoldingsStatementViewModel> _holdingsStatements = new ObservableCollection<HoldingsStatementViewModel>();
		public PortfolioViewModel()
		{
		}
		public int Id { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
		public ObservableCollection<HoldingsStatementViewModel> HoldingsStatements { get { return _holdingsStatements; } }

		public int? NumberOfConstituents { get; set; }
	}
}
