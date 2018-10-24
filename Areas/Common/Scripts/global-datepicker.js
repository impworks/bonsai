function setupDatepicker($d, allowFuzzy) {
    $d.datepicker({
        locale: 'ru-ru',
        uiLibrary: 'bootstrap4',
        format: 'yyyy.mm.dd'
    });

    $d.prop('autocomplete', 'off');
    $d.tooltip({
        title: 'Недопустимая дата'
    });

    $d.on('click', function () {
        $d.open();
    });

    validate();
    $d.on('change', validate);

    return $d;

    function validate() {
        var valid = isValid($d.val());
        $d.toggleClass('is-invalid', !valid)
          .tooltip(valid ? 'disable': 'enable');
    }

    function isValid(value) {
        if (!value)
            return true;

        var goodRegex = allowFuzzy
            ? /(\?{4}|[\d]{3}[\d?])\.(\d\d|\?\?)\.(\d\d|\?\?)/
            : /\d{4}\.\d{2}\.\d{2}/;

        var badRegexes = [
            /\?{4}\.(\?\?\...|\...\?\?})/,
            /\d{4}\.\?\?\.\d{2}/
        ];

        if (!goodRegex.test(value) || badRegexes.some(function (x) { return x.test(value); }))
            return false;

        var parts = value.split('.');
        var possible = (parseInt(parts[0]) || 2000) + '.' + (parseInt(parts[1]) || 1) + '.' + (parseInt(parts[2]) || 1);
        return !isNaN(new Date(possible).getTime());
    }
}

$(function () {
    setupDatepicker($('.datepicker'));
})