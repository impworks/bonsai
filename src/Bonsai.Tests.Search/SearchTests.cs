using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Tests.Search.Fixtures;
using Xunit;

namespace Bonsai.Tests.Search
{
    [Collection("Search tests")]
    public class SearchTests
    {
        public SearchTests(SearchEngineFixture ctx)
        {
            _ctx = ctx;
        }

        private readonly SearchEngineFixture _ctx;

        [Fact]
        public async Task Search_finds_exact_matches()
        {
            var query = "Иванов Иван Петрович";

            var result = await _ctx.Search.SearchAsync(query);

            Assert.Contains(result, x => x.Key == "Иванов_Иван_Петрович");
        }

        [Fact]
        public async Task Search_considers_page_body()
        {
            var query = "Авиастроения";

            var result = await _ctx.Search.SearchAsync(query);

            Assert.Contains(result, x => x.Key == "Иванов_Иван_Петрович");
        }

        [Theory]
        [InlineData("Иванов", "Иванов.*")]
        [InlineData("Иванова", "Иванов.*")]
        public async Task Search_highlights_matches_in_title(string query, string regex)
        {
            var result = await _ctx.Search.SearchAsync(query);

            Assert.NotEmpty(result);
            Assert.All(result, x => Assert.True(Regex.IsMatch(x.HighlightedTitle, "<b>" + regex + "</b>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)));
        }

        [Theory]
        [InlineData("Иванов", "Иванов.*")]
        [InlineData("Иванова", "Иванов.*")]
        public async Task Search_highlights_matches_in_body(string query, string regex)
        {
            var result = await _ctx.Search.SearchAsync(query);

            Assert.NotEmpty(result);
            Assert.Contains(result, x => Regex.IsMatch(x.HighlightedDescription, "<b>" + regex + "</b>", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));
        }

        [Fact]
        public async Task Search_considers_pagination()
        {
            var query = "Иванов";

            var p1 = await _ctx.Search.SearchAsync(query, 0);
            var p2 = await _ctx.Search.SearchAsync(query, 1);

            Assert.Empty(p1.Select(x => x.Key).Intersect(p2.Select(x => x.Key)));
        }
    }
}
