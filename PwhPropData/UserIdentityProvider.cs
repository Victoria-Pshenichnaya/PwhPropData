using PwhPropData.Core.Interfaces;

namespace PwhPropData.Core
{
	public class UserIdentityProvider : IUserIdentityProvider
	{
		public UserIdentityProvider()
		{
			UserType = UserIdentityType.Uuid;
		}

		public string Uuid { get; set; }
		public UserIdentityType UserType { get; set; }
	}
}
