$(function () {
    var $loc = $('#Location'),
        $evt = $('#Event'),
        $date = $('#Date'),
        $upl = $('.media-uploader');

    $upl.find('input[type="file"]').fileupload({
        dataType: 'json',
        url: '/admin/media/upload',
        sequentialUploads: true,
        add: function (e, data) {
            var maxSize = +$upl.data('max-size');
            if (maxSize) {
                var size = data.originalFiles[0].size;
                if (size > maxSize) {
                    toastr.error('Файл слишком большой!');
                    return;
                }
            }
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

    setupPagePicker($evt, {
        create: true,
        placeholder: 'Страница или название',
        types: [2]
    });
    setupPagePicker($loc, {
        create: true,
        placeholder: 'Страница или название',
        types: [3]
    });

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
});