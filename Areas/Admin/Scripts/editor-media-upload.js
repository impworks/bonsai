$(function() {
    $('.media-uploader input[type="file"]').fileupload({
        dataType: 'json',
        url: '/admin/media/upload',
        sequentialUploads: true,
        add: function (e, data) {
            data.context = createUploadItem();
            data.submit();
        },
        done: function (e, data) {
            var body = JSON.parse(data.response().jqXHR.responseText);
            if (body.error) {
                displayError(data.context, body.description);
            } else {
                displaySuccess(data.context, body);
            }
        },
        fail: function (e, data) {
            displayError(data.context);
        },
        progress: function(e, data) {
            var prog = parseInt(data.loaded / data.total * 100, 10);
            data.context.find('progress').text(prog + '%');
        }
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
});