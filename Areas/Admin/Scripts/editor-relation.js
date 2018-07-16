$(function() {
    if ($('#relation-editor-form').length === 0)
        return;

    var $typeRow = $('.form-row[data-tier="type"]'),
        $typeEditor = $typeRow.find('select'),
        $sourceRow = $('.form-row[data-tier="source"]'),
        $sourceEditor = $sourceRow.find('select'),
        $destRow = $('.form-row[data-tier="destination"]'),
        $destEditor = $destRow.find('select'),
        $eventRow = $('.form-row[data-tier="event"]'),
        $eventEditor = $eventRow.find('select'),
        $durationRow = $('.form-row[data-tier="duration"]'),
        $durationStartEditor = $durationRow.find('input.duration-start'),
        $durationEndEditor = $durationRow.find('input.duration-end');

    var types = {
        source: [],
        dest: []
    };

    $typeEditor.selectize({
        openOnFocus: true,
        maxOptions: 100,
        onChange: function (value) {
            clear($sourceEditor);
            clear($destEditor);
            refreshProperties(value);
        }
    });

    setupPagePicker($sourceEditor, 'source');
    setupPagePicker($destEditor, 'dest');
    setupPagePicker($eventEditor, [2]);

    setupDatePicker($durationStartEditor, null, $durationEndEditor);
    setupDatePicker($durationEndEditor, $durationStartEditor, null);

    prepopulate();

    function prepopulate() {
        // loads initial types
        var type = $typeEditor[0].selectize.items[0];
        if (typeof type === 'string') {
            refreshProperties(type);
        }
    }

    function refreshProperties(type) {
        $.ajax({
                url: '/admin/relations/editorProps',
                data: { relType: type }
            })
            .done(function (data) {
                types.source = data.sourceTypes;
                types.dest = data.destinationTypes;

                $sourceRow.find('label').text(data.sourceName);
                $destRow.find('label').text(data.destinationName);
                $durationRow.toggle(!!data.showDuration);
                $eventRow.toggle(!!data.showEvent);

                preload($sourceEditor, 'source');
                preload($destEditor, 'dest');
            });
    }

    function preload($select, typesDef) {
        // loads data according to current selected value
        var s = $select[0].selectize;
        var curr = s.items.length > 0 ? s.options[s.items[0]] : {};
        var query = curr.title || '';
        s.load(function(callback) {
            loadData(query, typesDef, callback);
        });
    }

    function clear($select) {
        // clears dropdown
        var s = $select[0].selectize;
        s.clear();
        s.clearOptions();
        s.renderCache = {};
    }

    function loadData(query, typesDef, callback) {
        // loads data according to current query
        var currTypes = typeof typesDef === 'string' ? types[typesDef] : typesDef;
        var url = '/admin/suggest/pages?query=' + encodeURIComponent(query);
        currTypes.forEach(function (t) { url += '&types=' + encodeURIComponent(t); });

        $.ajax({
                url: url
            })
            .done(function (data) {
                callback(data);
            });
    }

    function setupPagePicker($elem, typesDef) {
        $elem.selectize({
            maxOptions: 10,
            maxItems: 1,
            openOnFocus: true,
            valueField: 'id',
            labelField: 'title',
            sortField: 'title',
            searchField: 'title',
            placeholder: 'Введите название страницы',
            preload: true,
            load: function (query, callback) {
                loadData(query, typesDef, callback);
            }
        });
    }

    function setupDatePicker($elem, $prev, $next) {
        $elem.datepicker({
            locale: 'ru-ru',
            uiLibrary: 'bootstrap4',
            format: 'yyyy.mm.dd',
            minDate: function () {
                return $prev != null ? $prev.val() : null;
            },
            maxDate: function() {
                return $next != null ? $next.val() : null;
            }
        });
    }
});