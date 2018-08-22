$(function() {
    $(document).on('click', '[data-tab-track]', function() {
        var $this = $(this);
        var field = $this.data('tab-track');
        var value = $this.data('tab-track-id');
        $('#' + field).val(value);
    });
});