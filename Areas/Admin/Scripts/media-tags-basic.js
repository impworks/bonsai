$(function() {
    var $wrap = $('.media-editor-tags-wrapper');
    if ($wrap.length === 0) {
        return;
    }

    var $entitiesField = $('#DepictedEntities');

    setupPagePicker($('#Location'), {
        create: true,
        types: [3],
        placeholder: 'Страница или название'
    });

    setupPagePicker($('#Event'), {
        create: true,
        types: [2],
        placeholder: 'Страница или название'
    });

    setupPagePicker($('#media-editor-tags-list'), {
        create: true,
        types: [0, 1, 4],
        onChange: function () {
            var self = this;
            var result = self.getValue().map(function(v) {
                return {
                    PageId: v,
                    ObjectTitle: self.getItem(v).text()
                };
            });
            var data = JSON.stringify(result);
            $entitiesField.val(data);
        }
    });
});