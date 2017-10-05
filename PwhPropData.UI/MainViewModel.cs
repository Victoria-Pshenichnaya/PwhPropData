using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using PwhPropData.Core;
using PwhPropData.Core.Converters;
using PwhPropData.Core.Entities;
using PwhPropData.Core.Interfaces;
using PwhPropData.Core.Managers;
using PwhPropData.Core.Storages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PwhPropData.UI
{
	public class MainViewModel : BindableBase
	{
		private readonly IPwhPropDataManager _pwhPropDataManager = null;
		private readonly IPwhManager _pwhManager = null;
		private readonly IAdcManager _adcManager = null;

		private DelegateCommand _selectedPortfolioChangedCommand = null;
		private DelegateCommand _updateRecommendationsCommand = null;
		private DelegateCommand _deleteApmPortfoliosCommand = null;
		private DelegateCommand _addApmPortfolioCommand = null;

		private PortfolioViewModel _selectedPortfolio = null;
		private HoldingsStatementViewModel _selectedHoldingsStatement = null;

		private ObservableCollection<PortfolioViewModel> _fundedPortfolios = new ObservableCollection<PortfolioViewModel>();

		private bool _isLoading = false;
		private string _messages = string.Empty;

		public MainViewModel()
		{
			ILogger logger = new Logger();
			_pwhManager = new PwhManager(logger, new PwhStorage(logger, new PwhConverter(new UserIdentityProvider())));
			_adcManager = new AdcManager(logger, new AdcStorage(logger));
			_pwhPropDataManager = new PwhPropDataManager(logger, _pwhManager, _adcManager);

			FillApmPortfolios();
		}

		public string Messages
		{
			get { return _messages; }
			set
			{
				SetProperty<string>(ref _messages, value);
			}
		}

		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				SetProperty<bool>(ref _isLoading, value);
				RaiseCommands();
			}
		}

		public DelegateCommand SelectedPortfolioChangedCommand
		{
			get
			{
				if (_selectedPortfolioChangedCommand == null)
				{
					_selectedPortfolioChangedCommand = new DelegateCommand(GetHoldingsStatements, () =>  SelectedPortfolio != null);
				}
				return _selectedPortfolioChangedCommand;
			}
		}
		public DelegateCommand UpdateRecommendationsCommand
		{
			get
			{
				if (_updateRecommendationsCommand == null)
				{
					_updateRecommendationsCommand = new DelegateCommand(UpdateRecommendations, () => !IsLoading);
				}
				return _updateRecommendationsCommand;
			}
		}

		public DelegateCommand DeleteApmPortfoliosCommand
		{
			get
			{
				if (_deleteApmPortfoliosCommand == null)
				{
					_deleteApmPortfoliosCommand = new DelegateCommand(DeleteApmPortfolios, () => !IsLoading);
				}
				return _deleteApmPortfoliosCommand;
			}
		}
		public DelegateCommand AddApmPortfolioCommand
		{
			get
			{
				if (_addApmPortfolioCommand == null)
				{
					_addApmPortfolioCommand = new DelegateCommand(AddApmPortfolio, () => !IsLoading);
				}
				return _addApmPortfolioCommand;
			}
		}

		public ObservableCollection<PortfolioViewModel> ApmFundedPortfolios
		{
			get
			{
				return _fundedPortfolios;
			}
		}

		public PortfolioViewModel SelectedPortfolio
		{
			get
			{
				return _selectedPortfolio;
			}
			set
			{
				SetProperty<PortfolioViewModel>(ref _selectedPortfolio, value);
			}
		}

		public HoldingsStatementViewModel SelectedHoldingsStatement
		{
			get
			{
				return _selectedHoldingsStatement;
			}
			set
			{
				SetProperty<HoldingsStatementViewModel>(ref _selectedHoldingsStatement, value);
			}
		}

		private async void FillApmPortfolios()
		{
			try
			{
				StartLoading("Portfolios");
				_fundedPortfolios.Clear();
				await _pwhManager.GetHeadersByAttributeNameAsync(Settings.PwhApmAttribute).ContinueWith(task =>
				{
					try
					{
						IEnumerable<PortfolioHeader> portfolios = task.Result;
						if (portfolios != null)
						{
							App.Current.Dispatcher.Invoke((Action)delegate
							{
								AddMessage($"{portfolios.Count()} APM portfolio(s) was(were) received.");
								foreach (var portfolio in portfolios)
								{
									_fundedPortfolios.Add(
										new PortfolioViewModel()
										{
											Id = portfolio.Id,
											Name = portfolio.Name,
											Code = portfolio.Code,
											HoldingsStatementsCount = portfolio.NumberOfConstituents
										});
								}
							});
						}
					}
					catch (Exception ex)
					{
						HandleException(ex, ex.Message);
					}
				});
			}
			finally
			{
				EndLoading("Portfolios");
			}
		}

		private async void GetHoldingsStatements()
		{
			try
			{
				if (SelectedPortfolio.HoldingsStatements.Count > 0)
				{
					return;
				}
				int portfolioId = SelectedPortfolio.Id;
				StartLoading($"Holdings Statements for portfolio {portfolioId}");
				await _pwhManager.GetHoldingsStatementsAsync(portfolioId, DateTime.Today.AddYears(-1)).ContinueWith(task =>
				{
					try
					{
						IEnumerable<HoldingsStatement> holdingStatements = task.Result;
						if (holdingStatements != null)
						{
							AddMessage($"{holdingStatements.Count()} holdings statement(s) was(were) received for portfolio {portfolioId}.");
							App.Current.Dispatcher.Invoke((Action)delegate
							{
								foreach (var holdingStatement in holdingStatements)
								{
									SelectedPortfolio.HoldingsStatements.Add(new HoldingsStatementViewModel()
									{
										Date = holdingStatement.HoldingsStatementDate,
										Holdings = holdingStatement.Holdings.Select(h => new HoldingDataViewModel() { CompanyId = h.CompanyId, Recommendation = h.Recommendation })
									});
								}
							});
						}
					}
					catch (Exception ex)
					{
						HandleException(ex, ex.Message);
					}
				});
			}
			finally
			{
				EndLoading("Portfolios");
			}
		}

		private async void DeleteApmPortfolios()
		{
			StartProcess("Start deleting...");
			try
			{
				await _pwhManager.DeletePortfolios(_fundedPortfolios.Select(p => p.Id));

				_fundedPortfolios.Clear();
			}
			catch (Exception ex)
			{
				HandleException(ex, ex.Message);
			}
			finally
			{
				EndProcess("End deleting.");
			}
		}

		private async void AddApmPortfolio()
		{
			StartProcess("Start adding...");
			try
			{
				Guid guid = Guid.NewGuid();
				int portfolioId = 0;
				await _pwhManager.AddApmPortfolioAsync(new FundedPortfolio() { Name = $"VP{guid}", Code = $"{guid}CODE" }).ContinueWith(task =>
				{
					try
					{
						portfolioId = task.Result;
						AddMessage($"APM portfolio with Id = {portfolioId} was added.");
					}
					catch (Exception ex)
					{
						HandleException(ex, ex.Message);
					}
				});

				IList<HoldingsStatement> holdingsStatements = new List<HoldingsStatement>();
				holdingsStatements.Add(new HoldingsStatement()
				{
					HoldingsStatementDate = DateTime.Today.AddDays(-1),
					LastModifiedDateTime = null,
					Holdings = new List<HoldingData>()
							{
								new HoldingData() { CompanyId = "HON.N", Recommendation = 2 },
								new HoldingData() { CompanyId = "FB.O", Recommendation = 1 },
								new HoldingData() { CompanyId = "CL.Z", Recommendation = 3 },
								new HoldingData() { CompanyId = "CL.N", Recommendation = 5 }
							}
				});

				await _pwhManager.AddHoldingsStatementsAsync(portfolioId, holdingsStatements);

				await _pwhManager.GetHeadersByPartfolioIdAsync(portfolioId).ContinueWith(task =>
				{
					try
					{
						IEnumerable<PortfolioHeader> portfolios = task.Result;
						if ((portfolios != null) && (portfolios.Count() > 0))
						{
							App.Current.Dispatcher.Invoke((Action)delegate
							{
								AddMessage($"APM portfolio with Id = {portfolios.First().Id} was received from PWH.");
								AddFundedPortfolio(portfolios.First());
							});
						}
					}
					catch (Exception ex)
					{
						HandleException(ex, ex.Message);
					}
				});
			}
			catch (Exception ex)
			{
				HandleException(ex, ex.Message);
			}
			finally
			{
				EndProcess("End adding.");
			}
		}

		private void AddFundedPortfolio(PortfolioHeader portfolio)
		{
			_fundedPortfolios.Add(
				new PortfolioViewModel()
				{
					Id = portfolio.Id,
					Name = portfolio.Name,
					Code = portfolio.Code,
					HoldingsStatementsCount = portfolio.NumberOfConstituents
				});
		}

		private async void UpdateRecommendations()
		{
			StartLoading($"Update recommendations");
			foreach (PortfolioViewModel portfolio in _fundedPortfolios)
			{
				try
				{
					//Get recommendations for portfolio
					IEnumerable<KeyValuePair<string, double>> recommenadations = _adcManager.GetRecommendations(portfolio.Id);

					if ((recommenadations != null) && (recommenadations.Count() > 0))
					{
						AddMessage($"For portfolio {portfolio.Id} was(were) received {recommenadations.Count()} recommendations.");
						foreach (var rec in recommenadations)
						{
							AddMessage($"{rec.Key} - {rec.Value}");
						}

						//Prepare holdings statement with recommendations
						IEnumerable<HoldingsStatement> holdingsStatementWithRecommendations = new List<HoldingsStatement>()
						{
							new HoldingsStatement()
							{
								HoldingsStatementDate = DateTime.Now,
								Holdings = recommenadations.Select(r => new HoldingData() { CompanyId = r.Key, Recommendation = r.Value })
							}
						};

						//Get Holdings Statements for today
						DateTime holdingsStatementDate = DateTime.Today;
						await _pwhManager.GetHoldingsStatementsAsync(portfolio.Id, holdingsStatementDate).ContinueWith(async (task) =>
						{
							IEnumerable<HoldingsStatement> holdingsStatements = task.Result;
							if ((holdingsStatements != null) && (holdingsStatements.Count() > 0))
							{
								//If there is already holdings statement for today then update this one with new recommendations
								await _pwhManager.UpdateHoldingStatementsAsync(portfolio.Id, holdingsStatementWithRecommendations);
							}
							else
							{
								//Add recommendations to pwh
								await _pwhManager.AddHoldingsStatementsAsync(portfolio.Id, holdingsStatementWithRecommendations);
							}
						});
					}
				}
				catch (Exception ex)
				{
					HandleException(ex, ex.Message);
				}
			}

			foreach (PortfolioViewModel portfolio in _fundedPortfolios)
			{
				portfolio.HoldingsStatements.Clear();
			}
			EndLoading($"Update recommendations");
		}

		private void AddMessage(string message)
		{
			if (!string.IsNullOrEmpty(Messages))
			{
				Messages = Messages.Insert(0, Environment.NewLine);
			}
			Messages = Messages.Insert(0, $"{DateTime.Now} -->{message}");
		}

		private void StartLoading(string message)
		{
			IsLoading = true;
			AddMessage($"{message} is(are) loading..");
		}

		private void EndLoading(string message)
		{
			IsLoading = false;
			AddMessage($"{message} loading is(are) finished.");
		}

		private void StartProcess(string message)
		{
			IsLoading = true;
			AddMessage(message);
		}

		private void EndProcess(string message)
		{
			IsLoading = false;
			AddMessage(message);
		}

		private void HandleException(Exception ex, string message)
		{
			Messages = Messages.Insert(0, message + Environment.NewLine);

			if ((ex != null) && (ex.InnerException != null))
			{
				HandleException(ex.InnerException, ex.InnerException.Message);
			}

			AggregateException agEx = ex as AggregateException;
			if (agEx != null)
			{
				foreach (var e in agEx.InnerExceptions)
				{
					if (e.InnerException != null)
					{
						HandleException(e.InnerException, e.InnerException.Message);
					}
				}
			}
		}
		private void RaiseCommands()
		{
			if (_updateRecommendationsCommand != null) _updateRecommendationsCommand.RaiseCanExecuteChanged();
			if (_selectedPortfolioChangedCommand != null) _selectedPortfolioChangedCommand.RaiseCanExecuteChanged();
			if (_addApmPortfolioCommand != null) _addApmPortfolioCommand.RaiseCanExecuteChanged();
			if (_deleteApmPortfoliosCommand != null) _deleteApmPortfoliosCommand.RaiseCanExecuteChanged();
		}
	}
}
