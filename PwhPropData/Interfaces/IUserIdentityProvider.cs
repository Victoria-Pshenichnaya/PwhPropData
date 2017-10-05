namespace PwhPropData.Core.Interfaces
{
	public interface IUserIdentityProvider
	{
		string Uuid { get; set; }
		UserIdentityType UserType { get; }
	}

	public enum UserIdentityType { SubId, Uuid };
}
