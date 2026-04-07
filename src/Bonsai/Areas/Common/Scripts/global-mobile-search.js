$(function () {
    var $header = $('.main-header');
    var $toggle = $('.mobile-search-toggle');
    var $close = $('.mobile-search-close');
    var $input = $('#search-query-mobile');

    // Auto-open if the mobile input already has a search value (e.g. after a search)
    if ($input.length && $input.val() && $(window).width() < 768) {
        $header.addClass('search-open');
    }

    $toggle.on('click', function () {
        $header.addClass('search-open');
        $input.focus();
    });

    $close.on('click', function () {
        $header.removeClass('search-open');
        $input.val('');
    });
});
