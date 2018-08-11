$(function () {
    var $d = $('.datepicker');

    $d.datepicker({
        locale: 'ru-ru',
        uiLibrary: 'bootstrap4',
        format: 'yyyy.mm.dd'
    });

    $d.prop('autocomplete', 'off');

    $d.on('click', function() {
        $d.open();
    });
})