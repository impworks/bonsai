$(function () {
    $('#search-query').autocomplete({
        minChars: 2,
        lookup: function (query, done) {
            var url = '/util/search/' + encodeURIComponent(query);
            $.ajax(url).then(
                function (result) {
                    var items = result.map(function (elem) {
                        return { value: elem.title, data: elem.key };
                    });
                    done({ suggestions: items });
                },
                function () {
                    done();
                }
            );
        },
        onSelect: function() {
            setTimeout(
                function () {
                    $('#search-query').closest('form').submit();
                },
                10
            );
        }
    });
});