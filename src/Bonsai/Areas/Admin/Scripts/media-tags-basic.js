$(function() {
    var $wrap = $('.media-editor-tags-wrapper');
    if ($wrap.length === 0) {
        return;
    }

    var $entitiesField = $('#DepictedEntities');

    setupPagePicker($('#Location'), {
        create: true,
        types: [3],
        placeholder: window.$bonsai.Js_Tags_PageOrTitle
    });

    setupPagePicker($('#Event'), {
        create: true,
        types: [2],
        placeholder: window.$bonsai.Js_Tags_PageOrTitle
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