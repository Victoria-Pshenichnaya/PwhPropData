namespace PwhPropData.Core
{
	public class Settings
	{
		public const string UUID = "PAXTRA80482";

		public const string PwhApmAttribute = "APM";
		public const string PwhApplicationCode = "PWT";

		public const string AdcUrl = "http://datacloud-beta.int.thomsonreuters.com:1080/snapshot/rest/select";
		public const string AdcRequestBody = "formula=TR.RecMean()&identifiers=P({portfolioId})&productid=PORTAL%3ACPVIEWS&output=Col%2C%20T%20%7Cva%2C%20Row%2C%20In%7C";
		public const string AdcRequestBodyPortfolioIdStr = "{portfolioId}";

	}
}
