using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Code.Services.Config;
using Microsoft.AspNetCore.WebUtilities;

namespace Bonsai.Areas.Mcp.Logic.Services;

/// <summary>
/// Issues and redeems one-time tokens that allow AI assistants to upload media files via a plain HTTP endpoint.
/// Tokens are kept in memory: a token lost on restart just means the assistant has to request a new one.
/// </summary>
public class MediaUploadTokenService(StaticConfig config)
{
    private readonly ConcurrentDictionary<string, MediaUploadTicket> _tickets = new();

    /// <summary>
    /// Creates a new one-time upload token.
    /// </summary>
    public MediaUploadTicket Create(string userId, string fileName, string mimeType, MediaUploadRequestVM request)
    {
        RemoveExpired();

        var ticket = new MediaUploadTicket
        {
            Token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32)),
            UserId = userId,
            FileName = fileName,
            MimeType = mimeType,
            Request = request,
            ExpiresAt = DateTimeOffset.Now.Add(config.Mcp?.EffectiveUploadTokenLifetime ?? McpConfig.DefaultUploadTokenLifetime)
        };

        _tickets[ticket.Token] = ticket;
        return ticket;
    }

    /// <summary>
    /// Consumes the token and returns its ticket, or null if the token is unknown, already used, or expired.
    /// </summary>
    public MediaUploadTicket TryRedeem(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        if (!_tickets.TryRemove(token, out var ticket))
            return null;

        return ticket.ExpiresAt > DateTimeOffset.Now ? ticket : null;
    }

    /// <summary>
    /// Removes expired tickets to keep memory usage bounded.
    /// </summary>
    private void RemoveExpired()
    {
        var now = DateTimeOffset.Now;
        foreach (var pair in _tickets)
            if (pair.Value.ExpiresAt <= now)
                _tickets.TryRemove(pair.Key, out _);
    }
}

/// <summary>
/// Details of a pending media upload.
/// </summary>
public class MediaUploadTicket
{
    public string Token { get; init; }
    public string UserId { get; init; }
    public string FileName { get; init; }
    public string MimeType { get; init; }
    public MediaUploadRequestVM Request { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
}
