$(function() {
    var $scr = $('#operation-message');
    if ($scr.length === 0) {
        return;
    }

    var body = $scr.html();
    var success = $scr.attr('success');
    toastr[success ? 'success' : 'error'](body);
});