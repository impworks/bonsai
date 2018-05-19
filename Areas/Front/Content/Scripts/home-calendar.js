$(function () {
    var prefix = 'cal-';
    var now = new Date();
    var selectedDay;
    var selectedClass = 'calendar-day-selected';

    function loadCalendar(year, month, day) {
        if (typeof year === "undefined")
            year = now.getFullYear();

        if (typeof month === "undefined")
            month = now.getMonth() + 1;

        if (typeof day === "undefined")
            day = now.getDate();

        selectedDay = year + '-' + month + '-' + day;

        loadMonth(year, month);
        loadDay(year, month, day);
    }

    function loadMonth(year, month) {
        var $wrapper = $('.calendar-wrapper .calendar-month'),
            url = '/util/cal/grid?year=' + year + '&month=' + month;

        $.ajax(url).then(
            function (html) {
                $wrapper.html(html);

                $('.calendar-day-active[data-date="' + selectedDay + '"]').addClass(selectedClass);

                updateEventsHeight();
            }
        );
    }

    function loadDay(year, month, day) {
        var $wrapper = $('.calendar-wrapper .calendar-events'),
            url = '/util/cal/list?year=' + year + '&month=' + month + '&day=' + day;

        $.ajax(url).then(
            function (html) {
                $wrapper.html(html);
                selectedDay = year + '-' + month + '-' + day;
                window.location.hash = prefix + selectedDay;

                $('.calendar-day').removeClass(selectedClass);
                $('.calendar-day-active[data-date="' + selectedDay + '"]').addClass(selectedClass);

                updateEventsHeight();
            }
        );
    }

    function updateEventsHeight() {
        var calendarHeight = $('.calendar-table').height();
        $('.calendar-events .calendar-events-content').css('max-height', calendarHeight + 'px');
    }

    if ($('.calendar-wrapper').length > 0) {

        var hash = window.location.hash;
        if (hash != null && hash.substr(1, prefix.length) === prefix) {
            var date = hash.substr(prefix.length + 1).split('-');
            loadCalendar(date[0], date[1], date[2]);
        } else {
            loadCalendar();
        }
        
        $('body').on('click', '.cmd-calendar-show', function (e) {
            var $elem = $(this),
                year = $elem.data('year'),
                month = $elem.data('month');

            loadMonth(year, month);

            e.preventDefault();
        }); 

        $('body').on('click', '.calendar-day-active', function (e) {
            var $elem = $(this),
                date = $elem.data('date').split('-');

            loadDay(date[0], date[1], date[2]);

            e.preventDefault();
        });
    }
});