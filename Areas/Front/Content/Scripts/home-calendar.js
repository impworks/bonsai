$(function() {
    function loadCalendar(year, month, isRetry) {
        var now = new Date();
        year = year || now.getFullYear();
        month = month || now.getMonth();

        var $wrapper = $('.calendar-wrapper'),
            url = '/c/' + year + '/' + month;

        $.ajax(url).then(
            function (html) {
                $wrapper.html(html);
                window.location.hash = 'c-' + year + '-' + month;
            },
            function () {
                // reload current
                if (!isRetry) {
                    loadCalendar(null, null, true);
                }
            }
        );
    }

    // toggle calendar on links
    $('body').on('click', '.calendar-link', function (e) {
        var $elem = $(this),
            year = $elem.data('year'),
            month = $elem.data('month');

        loadCalendar(year, month);

        e.preventDefault();
    });

    // display default or selected calendar
    var hash = window.location.hash;
    var prefix = 'cal-';

    if (hash != null && hash.substr(1, prefix.length) === prefix) {
        var date = hash.substr(prefix.length + 1).split('-');
        loadCalendar(date[0], date[1]);
    } else {
        loadCalendar();
    }
});