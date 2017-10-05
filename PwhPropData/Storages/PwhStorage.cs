using PwhPropData.Core.Common;
using PwhPropData.Core.Converters;
using PwhPropData.Core.Entities;
using PwhPropData.Core.Interfaces;
//using PwhPropData.Core.PwhService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading.Tasks;
using Pwh = PwhPropData.Core.PwhProxy.PwhService;//PwhPropData.Core.PwhService;

namespace PwhPropData.Core.Storages
{
	public class PwhStorage : IPwhStorage
	{
		private Pwh.PortfolioWarehouseClient _client = null;
		private Pwh.ClientDetails _clientDitails = null;

		private const string FundedPortfolioType = "FundedPortfolio";

		private IPwhConverter _pwhConverter = null;
		private ILogger _logger = null;

		public PwhStorage(ILogger logger, IPwhConverter pwhConverter)
		{
			_client = new Pwh.PortfolioWarehouseClient("PortfolioWarehouseUUID");

			_pwhConverter = pwhConverter;
			_logger = logger;
		}

		private Guid RequestId
		{
			get { return Guid.NewGuid(); }
		}

		private string ApplicationCode
		{
			get
			{
				return Settings.PwhApplicationCode;
			}
		}

		private Pwh.ClientDetails ClientDetails
		{
			get
			{
				if (_clientDitails == null)
				{
					_clientDitails = new Pwh.ClientDetails()
					{
						MachineDetails = GetUniqueHwId(),
						ProductVersion = GetProductVersion()
					};
				}
				return _clientDitails;
			}
		}

		private Pwh.UserIdentity UserIdentity
		{
			get
			{
				return new Pwh.UserIdentity()
				{
					UUID = Settings.UUID
				};
			}
		}

		public async Task<int> AddFoundPortfolioAsync(Entities.FundedPortfolio portfolio)
		{
			try
			{
				var fundedPortfolio = _pwhConverter.ConvertFundedPortfolio(portfolio);

				var request = new Pwh.CreateFundedPortfolioRequestMessage_V4()
				{
					ApplicationCode = ApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					FundedPortfolio = fundedPortfolio,
					userIdentity = UserIdentity
				};

				Pwh.CreateFundedPortfolioResponseMessage_V3 response = await _client.CreateFundedPortfolioAsync(request);
				return response.PortfolioId;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				throw ex;
			}
		}

		public async Task AddHoldingsStatementsAsync(int portfolioId, IEnumerable<Entities.HoldingsStatement> holdingStatements)
		{
			try
			{
				Pwh.HoldingsStatements pwhStatements = new Pwh.HoldingsStatements();
				pwhStatements.AddRange(holdingStatements.Select(_pwhConverter.ConvertHoldingsStatement));

				var request = new Pwh.CreateHoldingsStatementsRequestMessage_V4()
				{
					ApplicationCode = Settings.PwhApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					PortfolioId = portfolioId,
					HoldingsStatements = pwhStatements,
					userIdentity = UserIdentity
				};

				Pwh.CreateHoldingsStatementsResponseMessage_V4 response = await _client.CreateHoldingsStatementsAsync(request);
				CheckResponseStatus(response.BulkOperationDateStatuses);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
				throw ex;
			}
		}

		public async Task<IEnumerable<HoldingsStatement>> GetHoldingsStatementsAsync(int portfolioId, DateTime startDate, DateTime endDate)
		{
			try
			{
				var request = new Pwh.GetHoldingsStatementsRequestMessage_V4()
				{
					ApplicationCode = ApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					userIdentity = UserIdentity,
					PortfolioId = portfolioId,
					HoldingsStatementGetOptions = new Pwh.HoldingsStatementGetOptions()
					{
						StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified),
						EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified),
						BestAvailableStatement = false,
						SplitAdjustToCurrent = false
					}
				};

				Pwh.GetHoldingsStatementsResponseMessage_V4 response = await _client.GetHoldingsStatementsAsync(request);

				return response.HoldingsStatements.Select(item => new HoldingsStatement()
				{
					HoldingsStatementDate = item.HoldingsStatementHeader.HoldingsStatementDate,
					LastModifiedDateTime = item.HoldingsStatementHeader.LastModifiedDateTime,
					Holdings = item.Holdings.Select(h => new HoldingData() { CompanyId = h.SourceSymbol, Recommendation = h.Units })
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw ex;
			}
		}

		public async Task UpdateHoldingStatementsAsync(int portfolioId, IEnumerable<HoldingsStatement> holdingStatements)
		{
			Guard.NotNull(holdingStatements, "holdingStatements");

			try
			{
				var request = new Pwh.UpdateHoldingsStatementsRequestMessage_V4()
				{
					ApplicationCode = ApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					userIdentity = UserIdentity,
					PortfolioId = portfolioId,
					HoldingsStatements = _pwhConverter.ConvertHoldingsStatements(holdingStatements)
				};

				Pwh.UpdateHoldingsStatementsResponseMessage_V4 response = await _client.UpdateHoldingsStatementsAsync(request);

				CheckResponseStatus(response.BulkOperationDateStatuses);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw ex;
			}
		}

		public async Task<IEnumerable<Entities.PortfolioHeader>> GetPortfolioHeadersAsync(IEnumerable<int> portfolioIds)
		{
			Pwh.PortfolioTypes types = new Pwh.PortfolioTypes();
			types.Add(FundedPortfolioType);

			Pwh.UserSources userSources = new Pwh.UserSources();
			userSources.Add(Pwh.UserSource.CurrentUser);

			Pwh.PortfolioIds ids = new Pwh.PortfolioIds();
			ids.AddRange(portfolioIds);

			var request = new Pwh.GetPortfolioHeadersByIdRequestMessage_V4
			{
				ApplicationCode = ApplicationCode,
				ClientDetails = ClientDetails,
				RequestId = RequestId,
				PortfolioIds = ids,
				userIdentity = UserIdentity
			};

			Pwh.GetPortfolioHeadersByIdResponseMessage_V4 result = await _client.GetPortfolioHeadersByIdAsync(request);

			return result.PortfolioHeaders == null ? null : result.PortfolioHeaders.Select(_pwhConverter.ConvertHeaders);
		}

		public async Task<IEnumerable<Entities.PortfolioHeader>> GetPortfolioHeadersByQueryAsync()
		{
			try
			{
				Pwh.PortfolioTypes types = new Pwh.PortfolioTypes();
				types.Add(FundedPortfolioType);

				Pwh.UserSources userSources = new Pwh.UserSources();
				userSources.Add(Pwh.UserSource.CurrentUser);

				var request = new Pwh.GetPortfolioHeadersByQueryRequestMessage_V4
				{
					ApplicationCode = ApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					PortfolioQueryOptions = new Pwh.PortfolioQueryOptions()
					{
						IdMatch = new Pwh.IdMatch() { IdField = Pwh.IdField.Any, IdMatchCondition = Pwh.IdMatchCondition.BeginsWith, IdValue = "" },
						SortField = Pwh.SortField.LastModifiedDateTime,
						MaximumRecordCount = 0,
						GetDefaultBenchmarkHeaders = false,
						PortfolioTypes = types,
						UserSources = userSources
					},
					userIdentity = UserIdentity
				};

				Pwh.GetPortfolioHeadersByQueryResponseMessage_V4 result = await _client.GetPortfolioHeadersByQueryAsync(request.ApplicationCode, request.ClientDetails, request.RequestId, request.userIdentity, request.PortfolioQueryOptions);
				return result.PortfolioHeaders == null ? null : result.PortfolioHeaders.Select(_pwhConverter.ConvertHeaders);
			}
			catch (FaultException ex)
			{
				_logger.LogError($"Code: {ex.Code.Name}, Message: {ex.Message}");
				throw ex;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw ex;
			}
		}

		public async Task<IEnumerable<Entities.PortfolioHeader>> GetPortfolioHeadersByAttributeAsync(Entities.PortfolioAttribute att)
		{
			try
			{
				Pwh.PortfolioTypes types = new Pwh.PortfolioTypes();
				types.Add(FundedPortfolioType);

				Pwh.UserSources userSources = new Pwh.UserSources();
				userSources.Add(Pwh.UserSource.CurrentUser);

				Pwh.PortfolioAttributes attributes = new Pwh.PortfolioAttributes();
				attributes.Add(new Pwh.PortfolioAttribute() { ApplicationCode = att.ApplicationCode, AttributeName = att.Name, AttributeValue = att.Value });

				var request = new Pwh.GetPortfolioHeadersByAttributeRequestMessage_V4
				{
					ApplicationCode = ApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					PortfolioQueryByAttributeOptions = new Pwh.PortfolioQueryByAttributeOptions()
					{
						IdMatch = new Pwh.IdMatch() { IdField = Pwh.IdField.Any, IdMatchCondition = Pwh.IdMatchCondition.BeginsWith, IdValue = "" },
						SortField = Pwh.SortField.LastModifiedDateTime,
						MaximumRecordCount = 0,
						GetDefaultBenchmarkHeaders = false,
						PortfolioTypes = types,
						UserSources = userSources,
						PortfolioAttributes = attributes
					},
					userIdentity = UserIdentity
				};

				Pwh.GetPortfolioHeadersByAttributeResponseMessage_V4 result = await _client.GetPortfolioHeadersByAttributeAsync(request.ApplicationCode, request.ClientDetails, request.RequestId, request.userIdentity, request.PortfolioQueryByAttributeOptions);

				return result.PortfolioHeaders == null ? null : result.PortfolioHeaders.Select(_pwhConverter.ConvertHeaders);
			}
			catch (FaultException ex)
			{
				_logger.LogError($"Code: {ex.Code.Name}, Message: {ex.Message}");
				throw ex;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw ex;
			}
		}

		public async Task DeletePortfoliosAsync(IEnumerable<int> portfolioIds)
		{
			try
			{
				Pwh.PortfolioIds ids = new Pwh.PortfolioIds();
				ids.AddRange(portfolioIds);

				var request = new Pwh.DeletePortfoliosRequestMessage_V4
				{
					ApplicationCode = ApplicationCode,
					ClientDetails = ClientDetails,
					RequestId = RequestId,
					PortfolioIds = ids,
					userIdentity = UserIdentity
				};

				Pwh.DeletePortfoliosResponseMessage_V4 result = await _client.DeletePortfoliosAsync(request.ApplicationCode, request.ClientDetails, request.RequestId, request.userIdentity, request.PortfolioIds);
				CheckResponseStatus(result.BulkOperationListStatuses);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex.Message);
				throw ex;
			}
		}

		private static void CheckResponseStatus(IEnumerable<Pwh.BulkOperationBaseStatus> statuses)
		{
			ServerException[] errors = statuses
				.Where(s => s.Status == Pwh.OperationStatus.Failure)
				.Select(s => new ServerException(s.ErrorCode + ": " + s.ErrorMessage))
				.ToArray();

			if (errors.Length > 0)
				throw new AggregateException("The response from PWH service contains errors.", errors);
		}

		private static string GetUniqueHwId()
		{
			string cpuInfo = "NotFoundPerSeatId";
			try
			{
				ManagementObjectSearcher objMOS = new ManagementObjectSearcher("Select ProcessorID From Win32_Processor");
				ManagementObjectCollection objMOC = objMOS.Get();
				foreach (ManagementObject objMO in objMOC)
				{
					if (cpuInfo.Equals("NotFoundPerSeatId"))
					{
						cpuInfo = objMO.Properties["ProcessorID"].Value.ToString();
						break;
					}
				}
			}
			catch (Exception) { }
			string windowsIdentity = WindowsIdentity.GetCurrent().Name.ToString();
			return $"{windowsIdentity}-{System.Environment.MachineName}-{cpuInfo}";
		}

		private static string GetProductVersion()
		{
			return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
		}
	}
}
