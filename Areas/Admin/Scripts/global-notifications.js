$(function() {
    $('.cmd-dismiss-notification').on('click', function () {
        var $this = $(this);
        var id = $this.data('notification-id');
        $.ajax('/admin/util/hideNotification?id=' + id);
        $this.parent().fadeOut(300);
    });
});