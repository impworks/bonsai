function setupDatepicker($d) {
    $d.datepicker({
        locale: 'ru-ru',
        uiLibrary: 'bootstrap4',
        format: 'yyyy.mm.dd'
    });

    $d.prop('autocomplete', 'off');

    $d.on('click', function () {
        $d.open();
    });

    return $d;
}

$(function () {
    setupDatepicker($('.datepicker'));
})