namespace PwhPropData.Core.Interfaces
{
	public interface IUserIdentityProvider
	{
		string Uuid { get; }
		UserIdentityType UserType { get; }
	}

	public enum UserIdentityType { SubId, Uuid };
}
