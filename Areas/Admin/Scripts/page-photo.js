$(function () {
    var $editor = $('#editor-main-photo');
    if ($editor.length === 0) {
        return;
    }

    var $wrap = $('.main-photo-wrapper'),
        $key = $('#MainPhotoKey'),
        $title = $('#Title'),
        $input = $('#main-photo-file'),
        $removeBtn = $editor.find('.cmd-remove');

    $editor.find('.cmd-upload').on('click', function() {
        $input.click();
    });

    $editor.find('.cmd-pick').on('click', function() {
        pickMedia([0], function(data) {
            setImage(data.thumbnailUrl, data.key);
        });
    });

    $input.fileupload({
        url: '/admin/media/upload',
        sequentialUploads: true,
        add: function (e, data) {
            data.formData = {
                title: $title.val()
            };
            data.submit();
            $input = $('#main-photo-file'); // gets new link to a recreated input
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
        $key.change();
        $removeBtn.show();
    }
});