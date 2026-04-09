$(function () {
    $('.sidebar-toggle').on('click', function () {
        var $details = $(this).siblings('.sidebar-details');
        var $icon = $(this).find('.fa');
        var isOpen = $details.hasClass('open');
        $details.toggleClass('open');
        $icon.toggleClass('fa-chevron-down', isOpen).toggleClass('fa-chevron-up', !isOpen);
    });
});
