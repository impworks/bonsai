$(function () {
    var prefix = 'cal-';

    function registerEvents() {
        $('.calendar-event').popover('dispose');

        // configure event
        $('.calendar-event').popover({
            container: 'body',
            trigger: 'manual',
            html: true,
            template: '<div class="popover" role="tooltip"><div class="arrow"></div><div class="popover-body"></div></div>',
            content: function () {
                var $det = $(this).find('.calendar-event-details');
                return $det.prop('outerHTML');
            }
        });

        $('body').on('click', '.calendar-event', function (e) {
            $(this).popover('show');
            e.stopPropagation();
        });

        $('body').on('mousedown', function(e) {
            var $wrapper = $('.popover');
            if (!$wrapper.is(e.target) && $wrapper.has(e.target).length === 0) {
                $wrapper.popover('hide');
            }
        });
    }

    function loadCalendar(year, month, isRetry) {
        var now = new Date();

        if (typeof year === "undefined")
            year = now.getFullYear();

        if (typeof month === "undefined")
            month = now.getMonth() + 1;

        var $wrapper = $('.calendar-wrapper'),
            url = '/c/' + year + '/' + month;

        $.ajax(url).then(
            function (html) {
                $wrapper.html(html);
                registerEvents();
                window.location.hash = prefix + year + '-' + month;
            },
            function () {
                // reload current
                if (!isRetry)
                    loadCalendar(null, null, true);
            }
        );
    }

    // toggle calendar on links
    $('body').on('click', '.cmd-calendar-show', function (e) {
        var $elem = $(this),
            year = $elem.data('year'),
            month = $elem.data('month');

        loadCalendar(year, month);

        e.preventDefault();
    });

    // display default or selected calendar
    var hash = window.location.hash;
    if (hash != null && hash.substr(1, prefix.length) === prefix) {
        var date = hash.substr(prefix.length + 1).split('-');
        loadCalendar(date[0], date[1]);
    } else {
        loadCalendar();
    }
});