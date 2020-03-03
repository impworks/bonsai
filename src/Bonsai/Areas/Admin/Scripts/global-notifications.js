$(function() {
    $('.cmd-dismiss-notification').on('click', function () {
        var $this = $(this);
        $this.parent().fadeOut(300);

        var id = $this.data('notification-id');
        if (id) {
            $.ajax('/admin/util/hideNotification?id=' + id);
        }
    });
});