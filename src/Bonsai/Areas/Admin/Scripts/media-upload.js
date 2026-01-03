$(function () {
    var $loc = $('#Location'),
        $evt = $('#Event'),
        $date = $('#Date'),
        $ufn = $('#UseFileNameAsTitle'),
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
                    toastr.error(window.$bonsai.Js_Upload_FileTooBig);
                    return;
                }
            }

            var file = data.originalFiles[0];
            data.context = createUploadItem();

            // Generate PDF preview if applicable
            if (file.type === 'application/pdf') {
                generatePdfPreview(file).then(function (previewBlob) {
                    submitUpload(data, previewBlob);
                }).catch(function () {
                    submitUpload(data, null);
                });
            } else {
                submitUpload(data, null);
            }
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
        placeholder: window.$bonsai.Js_Tags_PageOrTitle,
        types: [2]
    });
    setupPagePicker($loc, {
        create: true,
        placeholder: window.$bonsai.Js_Tags_PageOrTitle,
        types: [3]
    });

    function displayError($ctx, msg) {
        $ctx.find('.progress')
            .hide();

        $ctx.find('.error')
            .show() 
            .prop('title', msg || window.$bonsai.Js_Upload_UnknownError);
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

    function submitUpload(data, previewBlob) {
        data.formData = {
            Location: $loc.val(),
            Event: $evt.val(),
            Date: $date.val(),
            UseFileNameAsTitle: $ufn.is(':checked'),
        };

        if (previewBlob) {
            data.formData.preview = previewBlob;
        }

        data.submit();
    }

    function generatePdfPreview(file) {
        var PREVIEW_WIDTH = 1280;
        var PREVIEW_HEIGHT = 768;

        return file.arrayBuffer().then(function (arrayBuffer) {
            return pdfjsLib.getDocument({ data: arrayBuffer }).promise;
        }).then(function (pdf) {
            return pdf.getPage(1);
        }).then(function (page) {
            var viewport = page.getViewport({ scale: 1 });
            var scale = Math.min(PREVIEW_WIDTH / viewport.width, PREVIEW_HEIGHT / viewport.height);
            var scaledViewport = page.getViewport({ scale: scale });

            var canvas = document.createElement('canvas');
            canvas.width = scaledViewport.width;
            canvas.height = scaledViewport.height;

            return page.render({
                canvasContext: canvas.getContext('2d'),
                viewport: scaledViewport
            }).promise.then(function () {
                return canvas;
            });
        }).then(function (canvas) {
            return new Promise(function (resolve) {
                canvas.toBlob(resolve, 'image/jpeg', 0.9);
            });
        });
    }
});