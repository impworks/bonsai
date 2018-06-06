namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Types of authorization result.
    /// </summary>
    public enum LoginStatus
    {
        /// <summary>
        /// The auth credentials are invalid.
        /// </summary>
        Failed,

        /// <summary>
        /// User has been authorized successfully.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Administrator has blocked this account.
        /// </summary>
        LockedOut,

        /// <summary>
        /// The user account is not yet validated by the admin.
        /// </summary>
        Unvalidated,

        /// <summary>
        /// The user account has just been created, but validation is pending.
        /// </summary>
        NewUser
    }
}
