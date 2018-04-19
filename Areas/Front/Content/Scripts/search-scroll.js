$(function () {
    var $wrapper = $('.search-results'),
        $win = $(window),
        page = 1;

    function debounce(func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    };

    function isInView($elem) {
        var winTop = $win.scrollTop(),
            winBottom = winTop + $win.height(),
            elemTop = $elem.offset().top,
            elemBottom = elemTop + $elem.height();

        // does not account for elements bigger than the page
        // but that's not our case
        return elemTop >= winTop && elemBottom <= winBottom;
    }

    function loadMore() {
        var query = $('#search-query-hidden').val();
        var url = '/s/results?query=' + encodeURIComponent(query) + '&page=' + page;
        $.ajax(url).then(function(html) {
            $wrapper.append(html);
            page++;
            listenForScroll();
        });
    }

    function listenForScroll() {
        var $elem = $wrapper.find('li.search-result:last-of-type');

        if (isInView($elem)) {
            loadMore();
            return;
        }

        var handler = debounce(function() {
            if (isInView($elem)) {
                loadMore();
                $win.off('scroll', handler);
            }
        }, 200);

        $win.on('scroll', handler);
    }

    listenForScroll();
})