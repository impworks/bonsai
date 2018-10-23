function setupDatepicker($d, allowFuzzy) {
    $d.datepicker({
        locale: 'ru-ru',
        uiLibrary: 'bootstrap4',
        format: 'yyyy.mm.dd'
    });

    $d.prop('autocomplete', 'off');

    $d.on('click', function () {
        $d.open();
    });

    $d.on('blur', function() {
        var isValid = validate($d.val());
        $d.toggleClass('is-invalid', !isValid);
    });

    return $d;

    function validate(value) {
        var regex = allowFuzzy
            ? /(\?{4}|[\d]{3}[\d?])\.(\d\d|\?\?)\.(\d\d|\?\?)/
            : /\d{4}\.\d{2}\.\d{2}/;

        if (!regex.test(value))
            return false;

        var parts = value.split('.');
        var possible = (parseInt(parts[0]) || 2000) + '.' + (parseInt(parts[1]) || 1) + '.' + (parseInt(parts[2]) || 1);
        return !isNaN(new Date(possible).getTime());
    }
}

$(function () {
    setupDatepicker($('.datepicker'));
})