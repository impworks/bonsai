$(function () {
    var $wrap = $('.media-editor-tags-wrapper');
    if ($wrap.length === 0) {
        return;
    }

    var $entitiesField = $('#DepictedEntities');

    // disabled when the media type does not support it
    if ($('#media-editor-tags-list').length > 0) {
        return;
    }

    var wrapWidth = $wrap.outerWidth(),
        wrapHeight = $wrap.outerHeight();

    var $selectedTag = null;
    var template = $('#media-tag-template').html();
    var tagsData = JSON.parse($entitiesField.val());
    tagsData.forEach(createTag);

    $(document).on('mouseup', function (e) {
        // deselect element
        if ($selectedTag == null) {
            return;
        }

        if (!$selectedTag.is(e.target) && $selectedTag.has(e.target).length === 0) {
            $selectedTag.removeClass('active');
            $selectedTag = null;
        }
    });

    function syncTagSize($tag, tagData) {
        // calculates pecentage from absolute size
        var elemPos = $tag.position(),
            wrapWidth = $wrap.outerWidth(),
            wrapHeight = $wrap.outerHeight();

        var x = 1.0 * elemPos.left / wrapWidth,
            y = 1.0 * elemPos.top / wrapHeight,
            w = 1.0 * $tag.outerWidth() / wrapWidth,
            h = 1.0 * $tag.outerHeight() / wrapHeight;

        tagData.Coordinates = [x, y, w, h].map(function (x) { return Math.round(x * 1000) / 1000; }).join(';');

        updateInput();
    }

    function updateInput() {
        // updates the hidden field with current tag data
        $entitiesField.val(JSON.stringify(tagsData));
    }

    function createTag(tagData) {
        // adds a resizable / draggable tag to the photo
        var coords = (tagData.Coordinates || '').split(';');
        if (coords.length < 4) {
            return;
        }

        var $tag = $(template);
        $tag.appendTo($wrap);
        $tag.on('mousedown', function () {
            if ($selectedTag != null) {
                $selectedTag.removeClass('active');
            }

            $selectedTag = $tag;
            $tag.addClass('active');
        });
        $tag.css({
            left: Math.round(coords[0] * wrapWidth),
            top: Math.round(coords[1] * wrapHeight),
            width: Math.round(coords[2] * wrapWidth),
            height: Math.round(coords[3] * wrapHeight)
        });
        $tag.draggable({
            containment: 'parent',
            scroll: false,
            stop: function () {
                syncTagSize($tag, tagData);
            }
        });
        $tag.resizable({
            containment: 'parent',
            handles: "ne, nw, se, sw",
            minWidth: 48,
            minHeight: 48,
            stop: function () {
                syncTagSize($tag, tagData);
            }
        });
    }
})