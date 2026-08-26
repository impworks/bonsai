using System;
using System.Collections.Generic;
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
    private readonly Lock _lock = new();
    private Dictionary<OAuthKeyPurpose, RsaSecurityKey> _keys;

    /// <summary>
    /// Returns the persistent key of the specified purpose, generating and storing it on first use.
    /// </summary>
    public RsaSecurityKey GetKey(OAuthKeyPurpose purpose)
    {
        EnsureLoaded();
        return _keys[purpose];
    }

    /// <summary>
    /// Loads the keys of all purposes from the database exactly once per process.
    /// </summary>
    private void EnsureLoaded()
    {
        if (_keys != null)
            return;

        lock (_lock)
        {
            if (_keys != null)
                return;

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _keys = Enum.GetValues<OAuthKeyPurpose>()
                        .ToDictionary(x => x, x => new RsaSecurityKey(LoadOrCreateKey(db, x)));
        }
    }

    /// <summary>
    /// Reads the key of the specified purpose from the database, or generates and stores a new one.
    /// </summary>
    private static RSA LoadOrCreateKey(AppDbContext db, OAuthKeyPurpose purpose)
    {
        var rsa = RSA.Create(2048);

        if (TryImportStoredKey())
            return rsa;

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
            if (!TryImportStoredKey())
                throw;
        }

        return rsa;

        bool TryImportStoredKey()
        {
            var stored = db.OAuthKeys.FirstOrDefault(x => x.Purpose == purpose);
            if (stored == null)
                return false;

            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(stored.PrivateKey), out _);
            return true;
        }
    }
}
