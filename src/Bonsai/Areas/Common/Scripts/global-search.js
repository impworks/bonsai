$(function () {
    var $query = $('#search-query');
    $query.autocomplete({
        minChars: 2,
        lookup: function (query, done) {
            var url = '/util/suggest/' + encodeURIComponent(query);
            $.ajax(url).then(
                function (result) {
                    var items = result.map(function (elem) {
                        return { value: elem.title, highlighted: elem.highlightedTitle, data: elem.key };
                    });
                    done({ suggestions: items });
                },
                function () {
                    done();
                }
            );
        },
        formatResult: function (suggestion) {
            return suggestion.highlighted || suggestion.value || '';
        },
        onSelect: function() {
            setTimeout(
                function () { $query.closest('form').submit(); },
                10
            );
        }
    });
});