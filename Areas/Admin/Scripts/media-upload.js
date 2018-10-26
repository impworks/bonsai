$(function () {
    var $loc = $('#Location'),
        $evt = $('#Event'),
        $date = $('#Date');

    $('.media-uploader input[type="file"]').fileupload({
        dataType: 'json',
        url: '/admin/media/upload',
        sequentialUploads: true,
        add: function (e, data) {
            data.formData = {
                Location: $loc.val(),
                Event: $evt.val(),
                Date: $date.val()
            };
            data.context = createUploadItem();
            data.submit();
        },
        done: function (e, data) {
            var body = JSON.parse(data.response().jqXHR.responseText);
            if (body.error) {
                displayError(data.context, body.description);
            } else {
                displaySuccess(data.context, body);
                refreshThumbnail(data.context, body.id);
            }
        },
        fail: function (e, data) {
            displayError(data.context);
        },
        progress: function(e, data) {
            var percent = parseInt(data.loaded / data.total * 100, 10) + '%';
            data.context.find('.progress-bar')
                .css({width: percent})
                .text(percent);
        }
    });

    setupSelectize($evt, [2]);
    setupSelectize($loc, [3]);

    function displayError($ctx, msg) {
        $ctx.find('.progress')
            .hide();

        $ctx.find('.error')
            .show() 
            .prop('title', msg || 'Неизвестная ошибка');
    }

    function displaySuccess($ctx, info) {
        $ctx.find('.media-uploader-preview')
            .removeClass('default')
            .css('background-image', 'url(' + info.thumbnailPath + ')');

        $ctx.find('.progress')
            .hide();

        $ctx.find('.media-edit-link')
            .show()
            .prop('href', '/admin/media/update?id=' + info.id);
    }

    function createUploadItem() {
        var html = $('#uploader-item-template-progress').html();
        return $(html).appendTo('.media-uploader-items');
    }

    function refreshThumbnail($ctx, id) {
        $.ajax('/admin/media/thumbs?ids=' + encodeURIComponent(id))
            .done(function (raw) {
                var data = raw[0];
                if (data.isProcessed) {
                    $ctx.find('.media-uploader-preview')
                        .css('background-image', 'url(' + data.thumbnailPath + '?' + new Date().getTime() + ')');
                } else {
                    setTimeout(function() { refreshThumbnail($ctx, id); }, 5000);
                }
            });
    }

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
            placeholder: 'Страница или название',
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
                option_create: function (data, escape) {
                    return '<div class="create">' + escape(data.input) + ' <i>(без ссылки)</i></div>';
                }
            }
        });
    }

    function loadData(query, types, callback) {
        // loads data according to current query
        var url = '/admin/suggest/pages?query=' + encodeURIComponent(query);
        types.forEach(function (t) { url += '&types=' + encodeURIComponent(t); });

        $.ajax(url)
            .done(function (data) {
                callback(data);
            });
    }
});