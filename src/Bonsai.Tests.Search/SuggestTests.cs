using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Data.Models;
using Bonsai.Tests.Search.Fixtures;
using Impworks.Utils.Linq;
using Xunit;

namespace Bonsai.Tests.Search
{
    [Collection("Search tests")]
    public class SuggestTests
    {
        public SuggestTests(SearchEngineFixture ctx)
        {
            _ctx = ctx;
        }

        private readonly SearchEngineFixture _ctx;

        [Fact]
        public async Task Prefix_search_matches()
        {
            var query = "Иванов";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == "Иванов_Иван_Петрович");
        }

        [Theory]
        [InlineData("Вадимович", "Семенов_Николай_Вадимович")]
        [InlineData("Ирина", "Семенова_Ирина_Алексеевна")]
        public async Task Non_prefix_search_matches(string query, string key)
        {
            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == key);
        }

        [Fact]
        public async Task Family_name_matches_male_to_female()
        {
            var query = "Иванов";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == "Иванова_Екатерина_Валерьевна");
        }

        [Fact]
        public async Task Family_name_matches_female_to_male()
        {
            var query = "Иванова";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == "Иванов_Иван_Петрович");
        }

        [Theory]
        [InlineData("Алексеевны", "Семенова_Ирина_Алексеевна")]
        [InlineData("Алексеевне", "Семенова_Ирина_Алексеевна")]
        [InlineData("Алексеевной", "Семенова_Ирина_Алексеевна")]
        [InlineData("Вадимовича", "Семенов_Николай_Вадимович")]
        [InlineData("Вадимовичу", "Семенов_Николай_Вадимович")]
        [InlineData("Вадимовичем", "Семенов_Николай_Вадимович")]
        public async Task Patronym_inflections_match(string query, string key)
        {
            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == key);
        }

        [Fact]
        public async Task Letters_e_yo_are_interchangeable()
        {
            var query = "Пётр";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == "Иванов_Петр_Михайлович");
        }

        [Fact]
        public async Task Case_is_ignored()
        {
            var query = "иВАНОВ";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == "Иванов_Петр_Михайлович");
        }

        [Fact]
        public async Task Page_type_filter_works()
        {
            var query = "Семенова";

            var result = await _ctx.Search.SuggestAsync(query, new[] { PageType.Person });

            Assert.Contains(result, x => x.Key.Contains("Семенова", StringComparison.InvariantCultureIgnoreCase));
            Assert.DoesNotContain(result, x => x.Key.StartsWith("Свадьба", StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task Empty_result_is_returned_for_unknown_query()
        {
            var query = "Алевтина";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData("Ивонов", "Иванов_Иван_Петрович")]
        [InlineData("Эванов", "Иванов_Иван_Петрович")]
        [InlineData("Иваноф", "Иванов_Иван_Петрович")]
        [InlineData("Олех", "Михайлов_Олег_Евгеньевич")]
        [InlineData("Мехайлов", "Михайлов_Олег_Евгеньевич")]
        [InlineData("Сименов", "Семенов_Евгений_Иванович")]
        public async Task Family_name_allows_one_typo(string query, string key)
        {
            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == key);
        }

        [Theory]
        [InlineData("семенова", 2)]
        [InlineData("семенов", 5)]
        [InlineData("иванов", 3)]
        public async Task Exact_matches_go_first(string query, int count)
        {
            var result = await _ctx.Search.SuggestAsync(query);

            Assert.True(result.Count >= count, "result.Count >= count");
            Assert.All(result.Take(count), x => Assert.StartsWith(query, x.Key, StringComparison.InvariantCultureIgnoreCase));
        }

        [Theory]
        [InlineData("Семенова Анна Николаевна")]
        [InlineData("Анна Семенова")]
        [InlineData("Михайлов Олег Евгеньевич")]
        public async Task Autocomplete_doesnt_hide_while_typing(string query)
        {
            var emptyLengths = new List<int>();
            for (var i = 3; i < query.Length; i++)
            {
                var typing = query.Substring(0, i);
                var result = await _ctx.Search.SuggestAsync(typing);
                if (result.Count == 0)
                    emptyLengths.Add(i);
            }

            Assert.Equal("", emptyLengths.JoinString(", "));
        }

        [Fact]
        public async Task Family_name_doesnt_discard_patronym()
        {
            var query = "Михайлов";

            var result = await _ctx.Search.SuggestAsync(query);

            Assert.Contains(result, x => x.Key == "Михайлов_Олег_Евгеньевич");
            Assert.Contains(result, x => x.Key == "Иванов_Петр_Михайлович");
        }

        [Fact]
        public async Task Suggest_with_page_types_does_not_include_unrelated_matches()
        {
            var query = "Барсик";

            var result = await _ctx.Search.SuggestAsync(query, new[] {PageType.Person, PageType.Pet}, 5);

            Assert.Equal(1, result.Count);
            Assert.Equal(result[0].Key, "Барсик");
        }
    }
}
