using Xunit;

namespace Bonsai.Tests.Search.Fixtures
{
    [CollectionDefinition("Search tests")]
    public class SearchCollection: ICollectionFixture<SearchEngineFixture>
    {
    }
}
