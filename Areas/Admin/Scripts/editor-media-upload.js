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
            data.context.find('progress').text('Done.');
        },
        fail: function(e, data) {
            console.log(data);
            alert('failed!');
        },
        progress: function(e, data) {
            var prog = parseInt(data.loaded / data.total * 100, 10);
            data.context.find('progress').text(prog + '%');
        }
    });

    function createUploadItem() {
        var html = $('#uploader-item-template-progress').html();
        return $(html).appendTo('.media-uploader-items');
    }
});