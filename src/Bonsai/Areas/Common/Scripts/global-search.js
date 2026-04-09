$(function () {
    var $header = $('.main-header');
    var $toggle = $('.search-toggle');
    var $query = $('#search-query');

    function openMobileSearch() {
        $header.addClass('search-open');
        $query.focus();
    }

    function closeMobileSearch() {
        $header.removeClass('search-open');
    }

    $toggle.on('click', function () {
        openMobileSearch();
    });

    $query.on('blur', function () {
        if (window.matchMedia('(max-width: 767px)').matches && !$query.val()) {
            closeMobileSearch();
        }
    });

    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && $header.hasClass('search-open')) {
            closeMobileSearch();
        }
    });

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