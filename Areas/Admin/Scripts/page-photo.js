$(function () {
    var $editor = $('#editor-main-photo');
    if ($editor.length === 0) {
        return;
    }

    var $wrap = $('.main-photo-wrapper');
    var $key = $('#MainPhotoKey');
    var $input = $('#main-photo-file');
    var $removeBtn = $editor.find('.cmd-remove');

    $editor.find('.cmd-upload').on('click', function() {
        $input.click();
    });

    $input.fileupload({
        url: '/admin/media/upload',
        sequentialUploads: true,
        add: function(e, data) {
            data.submit();
        },
        done: function (e, data) {
            var body = JSON.parse(data.response().jqXHR.responseText);
            if (body.error) {
                toastr.error(body.description);
            } else {
                setImage(body.thumbnailPath, body.key);
            }
        },
        fail: function() {
            toastr.error('Не удалось загрузить изображение!');
        }
    });

    $removeBtn.on('click', function() {
        $wrap.find('.main-photo').remove();
        $key.val('');
        $removeBtn.hide();
    });

    function setImage(thumb, key) {
        var $img = $wrap.find('.main-photo');
        if ($img.length === 0) {
            $img = $('<img>').addClass('main-photo').appendTo($wrap);
        }

        $img.prop('src', thumb);
        $key.val(key);
        $removeBtn.show();
    }
});