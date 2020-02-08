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

    var justLoaded = true;
    setTimeout(function() { justLoaded = false; }, 1000);

    $typeEditor.selectize({
        openOnFocus: true,
        maxOptions: 100,
        onChange: function (value) {
            clear($sourceEditor);
            clear($destEditor);
            refreshProperties(value, true);
        }
    });

    setupRelPagePicker($sourceEditor, 'source');
    setupRelPagePicker($destEditor, 'dest');
    setupRelPagePicker($eventEditor, [2]);

    $sourceEditor.on('change', function () { preload($destEditor, 'dest'); });
    $destEditor.on('change', function () { preload($sourceEditor, 'source'); });

    setupDatePicker($durationStartEditor, null, $durationEndEditor);
    setupDatePicker($durationEndEditor, $durationStartEditor, null);

    prepopulate();

    function prepopulate() {
        // loads initial types
        var type = $typeEditor[0].selectize.items[0];
        if (typeof type === 'string') {
            refreshProperties(type, false);
        }
    }

    function refreshProperties(type, force) {
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

                if (force) {
                    preload($sourceEditor, 'source');
                    preload($destEditor, 'dest');
                }

                if (!justLoaded) {
                    $('.validation-result').hide();
                }
            });
    }

    function preload($select, typesDef) {
        // loads data according to current selected value
        var s = $select[0].selectize;
        var curr = s.items.length > 0 ? s.options[s.items[0]] : {};
        var query = curr.title || '';
        s.load(function(callback) {
            var url = getQueryUrl(query, typesDef);
            $.ajax(url).done(function (data) {
                s.clearOptions();
                s.renderCache = {};
                callback(data);
            });
        });
    }

    function clear($select) {
        // clears dropdown
        var s = $select[0].selectize;
        s.clear();
        s.clearOptions();
        s.renderCache = {};
    }

    function getQueryUrl(query, typesDef) {
        // loads data according to current query

        if (typeof typesDef === 'object') {
            return getUrl('/admin/suggest/pages', { query: query, types: typesDef });
        }

        var args = {
            query: query,
            types: types[typesDef],
            relationType: getSelected($typeEditor)
        };

        if (typesDef === 'source') {
            args.destinationId = getSelected($destEditor);
        } else {
            args.sourceId = getSelected($sourceEditor);
        }

        return getUrl('/admin/suggest/pages/rel', args);
    }

    function setupRelPagePicker($elem, typesDef) {
        setupPagePicker($elem, {
            placeholder: 'Введите название страницы',
            urlFactory: function(query) {
                return getQueryUrl(query, typesDef);
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

    function getUrl(root, args) {
        var parts = [];

        function addPart(key, value) {
            if (value === null || typeof value === 'undefined' || value === '') {
                return;
            }

            parts.push({ key: key, value: value });
        }

        for (var pty in args) {
            if (!args.hasOwnProperty(pty)) {
                continue;
            }

            var elem = args[pty];
            if (Array.isArray(elem)) {
                for (var i = 0; i < elem.length; i++) {
                    addPart(pty, elem[i]);
                }
            } else {
                addPart(pty, elem);
            }
        }

        if (parts.length === 0) {
            return root;
        }

        var enc = encodeURIComponent;
        var query = parts.map(function (e) { return enc(e.key) + "=" + enc(e.value); }).join("&");
        return root + "?" + query;
    }

    function getSelected($elem) {
        var items = $elem[0].selectize.items;
        return items.length > 0 ? items[0] : undefined;
    }
});