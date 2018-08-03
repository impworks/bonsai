$(function() {
    var $wrap = $('.media-editor-tags-wrapper');
    if ($wrap.length === 0) {
        return;
    }

    var $entitiesField = $('#DepictedEntities');

    var eventType = [2],
        locationType = [3],
        otherTypes = [0, 1, 4];

    setupSelectize($('#Location'), locationType);
    setupSelectize($('#Event'), eventType);
    setupSelectize($('#media-editor-tags-list'), otherTypes, function($s) {
        var data = JSON.stringify($s[0].selectize.getValue());
        $entitiesField.val(data);
    });

    setupDatePicker($('#Date'));

    function setupSelectize($select, types, handler) {
        var multiple = $select.prop('multiple');
        $select.selectize({
            create: true,
            maxOptions: 10,
            maxItems: multiple ? null : 1,
            openOnFocus: true,
            valueField: 'id',
            labelField: 'title',
            sortField: 'title',
            searchField: 'title',
            placeholder: 'Выберите страницу или введите название',
            preload: true,
            load: function (query, callback) {
                loadData(query, types, callback);
            },
            onChange: function () {
                if (!!handler) {
                    handler($select);
                }
            },
            render: {
                option_create: function(data, escape) {
                    return '<div class="create">' + escape(data.input) + ' <i>(без ссылки)</i></div>';
                }
            }
        });
    }

    function loadData(query, types, callback) {
        // loads data according to current query
        var url = '/admin/suggest/pages?query=' + encodeURIComponent(query);
        types.forEach(function (t) { url += '&types=' + encodeURIComponent(t); });

        $.ajax({ url: url })
            .done(function (data) {
                callback(data);
            });
    }

    function setupDatePicker($elem) {
        $elem.datepicker({
            locale: 'ru-ru',
            uiLibrary: 'bootstrap4',
            format: 'yyyy.mm.dd'
        });
    }
});