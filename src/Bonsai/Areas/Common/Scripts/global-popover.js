$(function() {
    $(document).popover({
        container: 'body',
        selector: '.popover-handle',
        html: true,
        placement: 'right',
        trigger: 'hover',
        content: function() {
            return $(this).closest('.popover-wrapper').find('.popover-contents').html()
        }
    });
})