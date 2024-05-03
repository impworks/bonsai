using Mapster;

namespace Bonsai.Code.Infrastructure;

/// <summary>
/// Configures automapper to map between types.
/// </summary>
internal interface IMapped
{
    void Configure(TypeAdapterConfig config);
}