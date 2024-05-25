namespace Bonsai.Areas.Front.Logic.Auth;

/// <summary>
/// Details of an external provider's authentication.
/// <param name="LoginProvider">External provider: Facebook, Google, etc.</param>
/// <param name="ProviderKey">User's personal key returned by the provider.</param>
/// </summary>
public record ExternalLoginData(string LoginProvider, string ProviderKey);