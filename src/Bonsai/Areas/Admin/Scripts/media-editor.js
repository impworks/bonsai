$(function() {
    var $form = $('.media-editor-form');
    if ($form.length === 0) {
        return;
    }

    var $submit = $form.find("button[type='submit']"),
        $input = $form.find('#SaveAction'),
        $dropItems = $form.find('.cmd-set-save-mode');

    $dropItems.on('click', function (e) {
        e.preventDefault();

        var $elem = $(this);

        $input.val($elem.data('value'));
        $submit.text($elem.text());

        $dropItems.removeClass('active');
        $elem.addClass('active');
    });

    $(document).on('keypress', function(e) {
        if (e.ctrlKey && (e.which === 10 || e.which === 13)) {
            $form.submit();
        }
    });
});