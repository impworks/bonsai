$(function () {
    var $select = $('#PersonalPageId');
    setupPagePicker($select);

    function setupPagePicker($elem) {
        $elem.selectize({
            maxOptions: 10,
            maxItems: 1,
            openOnFocus: true,
            valueField: 'id',
            labelField: 'title',
            sortField: 'title',
            searchField: 'title',
            placeholder: 'Введите имя',
            preload: true,
            load: function (query, callback) {
                var url = '/admin/suggest/pages?types=0&query=' + encodeURIComponent(query);
                $.ajax(url)
                    .done(function (data) {
                        callback(data);
                    });
            }
        });
    }
});