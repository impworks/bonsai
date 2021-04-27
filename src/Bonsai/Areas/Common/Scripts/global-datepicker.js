function setupDatepicker($d) {
    var allowFuzzy = !$d.is('.datepicker-strict');

    $d.datepicker({
        locale: 'ru-ru',
        uiLibrary: 'bootstrap4',
        format: 'yyyy.mm.dd',
        change: function(e) {
            console.log('changed: ', e);
        }
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

        var regexes = [/^\d{4}\.\d{2}\.\d{2}$/];
        if (allowFuzzy) {
            regexes.push(
                /^\d{4}\.\d{2}(.\?\?)?$/,
                /^\d{3}(\d|\?)(\.\?\?(.\?\?)?)?$/,
                /^\?{4}\.\d{2}.\d{2}$/
            );
        }

        if (!regexes.some(function(rx) { return rx.test(value); }))
            return false;

        var parts = value.split('.');
        var year = parseInt(parts[0]) || 2000;
        var month = parseInt(parts.length > 0 ? parts[1] : '') || 1;
        var day = parseInt(parts.length > 1 ? parts[2] : '') || 1;
        return !isNaN(new Date(year + '/' + month + '/' + day).getTime());
    }
}

$(function () {
    var oldParse = gj.core.parseDate;
    gj.core.parseDate = function (value, format, locale) {
        // don't force year-only format like '1990' into '1990.01.01'!
        return /\d{3}(\d|\?)/.test(value)
            ? new Date('')
            : oldParse(value, format, locale);
    };

    $('.datepicker').each(function() {
        setupDatepicker($(this));
    });
})