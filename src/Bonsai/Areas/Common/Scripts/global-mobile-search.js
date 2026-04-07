$(function () {
    var $header = $('.main-header');
    var $toggle = $('.mobile-search-toggle');
    var $input = $('#search-query-mobile');

    // Auto-open if the mobile input already has a search value (e.g. after a search)
    if ($input.length && $input.val() && $(window).width() < 768) {
        $header.addClass('search-open');
    }

    $toggle.on('click', function () {
        $header.addClass('search-open');
        // Small delay ensures the element has started expanding before focus
        setTimeout(function () { $input.focus(); }, 50);
    });
});
