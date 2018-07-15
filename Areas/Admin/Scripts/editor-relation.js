$(function() {
    if ($('#relation-editor-form').length === 0)
        return;

    var $typeRow = $('.form-row[data-tier="type"]'),
        $typeEditor = $typeRow.find('select'),
        $sourceRow = $('.form-row[data-tier="source"]'),
        $sourceEditor = $sourceRow.find('select'),
        $destRow = $('.form-row[data-tier="destination"]'),
        $destEditor = $destRow.find('select'),
        $durationRow = $('.form-row[data-tier="duration"]'),
        $durationEditor = $durationRow.find('input'),
        $eventRow = $('.form-row[data-tier="event"]'),
        $eventEditor = $eventRow.find('select');

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

                $sourceEditor[0].selectize.load(function (callback) { loadData('', 'source', callback); });
                $destEditor[0].selectize.load(function (callback) { loadData('', 'dest', callback); });
            });
    }

    function clear($select) {
        var s = $select[0].selectize;
        s.clear();
        s.clearOptions();
        s.renderCache = {};
    }

    function loadData(query, typesDef, callback) {
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

    setupPagePicker($sourceEditor, 'source');
    setupPagePicker($destEditor, 'dest');
    setupPagePicker($eventEditor, [2]);
});