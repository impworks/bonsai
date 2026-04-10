$(function () {
    $('.sidebar-toggle').on('click', function () {
        var $collapsed = $(this).siblings('.sidebar-collapsed');
        var $details = $(this).siblings('.sidebar-details');
        var $icon = $(this).find('.fa');
        var isOpen = $details.hasClass('open');
        $collapsed.toggleClass('d-none', !isOpen);
        $details.toggleClass('open');
        $icon.toggleClass('fa-chevron-down', isOpen).toggleClass('fa-chevron-up', !isOpen);
    });
});
