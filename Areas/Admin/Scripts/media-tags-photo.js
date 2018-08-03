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
    var tagTemplate = $('#media-tag-template').html();
    var popupTemplate = $('#media-tag-popup-template').html();
    var tagsData = JSON.parse($entitiesField.val());
    tagsData.forEach(createTag);

    $(document).on('mousedown', function (e) {
        // deselect element
        if ($selectedTag == null) {
            return;
        }

        if (!$selectedTag.is(e.target) && $selectedTag.has(e.target).length === 0) {
            selectTag(null);
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

        tagData.Coordinates = [x, y, w, h].map(function (v) { return Math.round(v * 1000) / 1000; }).join(';');

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

        var $tag = $(tagTemplate);
        $tag.appendTo($wrap);
        $tag.css({
            left: Math.round(coords[0] * wrapWidth),
            top: Math.round(coords[1] * wrapHeight),
            width: Math.round(coords[2] * wrapWidth),
            height: Math.round(coords[3] * wrapHeight)
        });
        $tag.draggable({
            containment: 'parent',
            scroll: false,
            handle: '.ui-draggable-handle',
            stop: function () {
                syncTagSize($tag, tagData);
            }
        });
        $tag.resizable({
            containment: 'parent',
            handles: "ne, nw, se, sw",
            minWidth: 48,
            minHeight: 48,
            resize: function() {
                $tag.popover('update');
            },
            stop: function () {
                syncTagSize($tag, tagData);
            }
        });

        createTagPopup(tagData, $tag);

        $tag.on('mousedown', function () {
            selectTag($tag);
        });
    }

    function createTagPopup(tagData, $tag) {
        // adds a popup that appears when the tag is active
        var $popup = $(popupTemplate),
            $select = $popup.find('select'),
            $removeBtn = $popup.find('.cmd-remove-tag');

        // sic! click doesn't work for some reason
        $removeBtn.on('mouseup', function () {
            // remove the tag
            $select[0].selectize.destroy();
            $tag.popover('dispose');
            $tag.remove();
        
            // remove the data from storage
            var idx = tagsData.indexOf(tagData);
            if (idx > -1) {
                tagsData.splice(idx, 1);
            }
        
            updateInput();
        });

        $select.selectize({
            create: true,
            maxOptions: 10,
            maxItems: 1,
            openOnFocus: true,
            valueField: 'id',
            labelField: 'title',
            sortField: 'title',
            searchField: 'title',
            placeholder: 'Страница или имя',
            preload: true,
            load: loadData,
            onChange: function () {
                var value = $select[0].selectize.getValue();
                if (isGuid(value)) {
                    tagData['PageId'] = value;
                    tagData['ObjectTitle'] = null;
                } else {
                    tagData['PageId'] = null;
                    tagData['ObjectTitle'] = value;
                }

                updateInput();
            },
            render: {
                option_create: function (data, escape) {
                    return '<div class="create">' + escape(data.input) + ' <i>(без ссылки)</i></div>';
                }
            }
        });

        $tag.popover({
            animation: false,
            container: $tag,
            html: true,
            content: $popup,
            placement: 'bottom',
            trigger: 'manual'
        });
    }

    function selectTag($tag) {
        if ($selectedTag != null) {
            $selectedTag.removeClass('active');
            $selectedTag.popover('hide');
        }

        $selectedTag = $tag;

        if ($selectedTag != null) {
            $selectedTag.addClass('active');
            $selectedTag.popover('show');
        }
    }

    function loadData(query, callback) {
        // loads data according to current query
        var types = [0, 1, 4];
        var url = '/admin/suggest/pages?query=' + encodeURIComponent(query);
        types.forEach(function (t) { url += '&types=' + encodeURIComponent(t); });

        $.ajax({ url: url })
            .done(function (data) {
                callback(data);
            });
    }

    function isGuid(value) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
    }
})