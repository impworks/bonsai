using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Bonsai.Areas.Mcp.Logic.Auth;

/// <summary>
/// Loads (or generates and persists) the RSA keys used by the OAuth server to sign and encrypt MCP tokens.
/// The keys are stored in the database so that already-authorized agents keep their access after a restart
/// instead of being forced to re-authorize.
/// </summary>
public class OAuthKeyManager(IServiceScopeFactory scopeFactory)
{
    private const string SigningPurpose = "Signing";
    private const string EncryptionPurpose = "Encryption";

    private readonly Lock _lock = new();
    private RsaSecurityKey _signingKey;
    private RsaSecurityKey _encryptionKey;

    /// <summary>
    /// Returns the persistent signing key, generating and storing it on first use.
    /// </summary>
    public RsaSecurityKey GetSigningKey()
    {
        EnsureLoaded();
        return _signingKey;
    }

    /// <summary>
    /// Returns the persistent encryption key, generating and storing it on first use.
    /// </summary>
    public RsaSecurityKey GetEncryptionKey()
    {
        EnsureLoaded();
        return _encryptionKey;
    }

    /// <summary>
    /// Loads both keys from the database exactly once per process.
    /// </summary>
    private void EnsureLoaded()
    {
        if (_signingKey != null && _encryptionKey != null)
            return;

        lock (_lock)
        {
            if (_signingKey != null && _encryptionKey != null)
                return;

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _signingKey = new RsaSecurityKey(LoadOrCreateKey(db, SigningPurpose));
            _encryptionKey = new RsaSecurityKey(LoadOrCreateKey(db, EncryptionPurpose));
        }
    }

    /// <summary>
    /// Reads the key of the specified purpose from the database, or generates and stores a new one.
    /// </summary>
    private static RSA LoadOrCreateKey(AppDbContext db, string purpose)
    {
        var rsa = RSA.Create(2048);

        var existing = db.OAuthKeys.FirstOrDefault(x => x.Purpose == purpose);
        if (existing != null)
        {
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(existing.PrivateKey), out _);
            return rsa;
        }

        db.OAuthKeys.Add(new OAuthKey
        {
            Purpose = purpose,
            PrivateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey()),
            CreatedAt = DateTimeOffset.UtcNow
        });

        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException)
        {
            // Another instance generated the key concurrently: discard ours and reuse the stored one.
            db.ChangeTracker.Clear();
            var stored = db.OAuthKeys.First(x => x.Purpose == purpose);
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(stored.PrivateKey), out _);
        }

        return rsa;
    }
}
