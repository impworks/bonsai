$(function() {
    var $wrap = $('.media-editor-tags-wrapper');
    if ($wrap.length === 0) {
        return;
    }

    var $entitiesField = $('#DepictedEntities');

    // disabled when the media type does not support it
    if ($('#media-editor-tags-list').length > 0) {
        return;
    }

    var MIN_SIZE = 48;

    var wrapWidth = $wrap.outerWidth(),
        wrapHeight = $wrap.outerHeight();

    var $selectedTag = null;
    var tagTemplate = $('#media-tag-template').html();
    var popupTemplate = $('#media-tag-popup-template').html();
    var tagsData = JSON.parse($entitiesField.val());
    var $tags = tagsData.map(createTag);

    handleDeselect();
    handleResize();
    handleNewTag();

    function syncTagSize($tag, tagData) {
        // calculates pecentage from absolute size
        var elemPos = $tag.position();

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
        if ((tagData.Coordinates || '').split(';').length < 4) {
            return null;
        }

        var $tag = $(tagTemplate);
        $tag.appendTo($wrap);
        positionTag($tag, tagData);
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
            minWidth: MIN_SIZE,
            minHeight: MIN_SIZE,
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

        return $tag;
    }

    function createTagPopup(tagData, $tag) {
        // adds a popup that appears when the tag is active
        var $popup = $(popupTemplate),
            $select = $popup.find('select'),
            $removeBtn = $popup.find('.cmd-remove-tag');

        // sic! click doesn't work for some reason
        $removeBtn.on('mouseup', function (e) {
            if (e.which !== 1) {
                return;
            }
            // remove the tag
            $select[0].selectize.destroy();
            $tag.popover('dispose');
            $tag.remove();
        
            // remove the data from storage
            var idx = tagsData.indexOf(tagData);
            if (idx > -1) {
                tagsData.splice(idx, 1);
                $tags.splice(idx, 1);
            }
        
            updateInput();
        });

        if (tagData.PageId || tagData.ObjectTitle) {
            $('<option>')
                .val(tagData.PageId)
                .text(tagData.ObjectTitle)
                .prop('selected', true)
                .appendTo($select);
        }

        setupPagePicker($select, {
            create: true,
            types: [0, 1, 4],
            onChange: function () {
                var value = this.getValue();
                if (isGuid(value)) {
                    tagData['PageId'] = value;
                    tagData['ObjectTitle'] = null;
                } else {
                    tagData['PageId'] = null;
                    tagData['ObjectTitle'] = value;
                }

                updateInput();
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
        var self = this;
        var types = [0, 1, 4];
        var url = '/admin/suggest/pages?query=' + encodeURIComponent(query);
        types.forEach(function (t) { url += '&types=' + encodeURIComponent(t); });

        $.ajax(url)
            .done(function (data) {
                // hack for fuzzy search:
                // cache values returned from server
                var cache = self.lastLoaded || (self.lastLoaded = {});
                cache[query] = data;
                self.lastQuery = null;
                callback(data);
            });
    }

    function handleResize() {
        $(window).on('resize', throttle(200, function (e) {
            if (e.target !== window) {
                return;
            }
            wrapWidth = $wrap.outerWidth();
            wrapHeight = $wrap.outerHeight();

            $tags.forEach(function($tag, idx) {
                positionTag($tag, tagsData[idx]);
            });
        }));
    }

    function handleDeselect() {
        $(document).on('mousedown', function(e) {
            // deselect element
            if ($selectedTag == null) {
                return;
            }

            if (!clickInside($selectedTag, e)) {
                selectTag(null);
            }
        });
    }

    function handleNewTag() {
        var isNewTagPending = false,
            isNewTagActive = false;

        var $img = $wrap.find('img'),
            $btn = $('.cmd-add-tag'),
            $doc = $(document),
            $tagFrame = null,
            tagAnchor = null,
            imgAnchor = $img.offset(),
            imgWidth = $img.outerWidth(),
            imgHeight = $img.outerHeight();

        $btn.on('click', function() {
            if (isNewTagPending || isNewTagActive) {
                return;
            }

            isNewTagPending = true;
            $wrap.addClass('new');
            $btn.addClass('active');
        });

        $doc.on('mousedown', function(e) {
            if (!isNewTagPending || e.which !== 1) {
                return;
            }

            if (!clickInside($img, e)) {
                isNewTagPending = false;
                $wrap.removeClass('new');
                $btn.removeClass('active');
                return;
            }

            tagAnchor = {
                top: e.pageY - imgAnchor.top,
                left: e.pageX - imgAnchor.left
            };

            $tagFrame = $('<div>')
                .addClass('photo-tag-editor new')
                .css(tagAnchor)
                .appendTo($wrap);

            isNewTagActive = true;
        });

        $doc.on('mousemove', function(e) {
            if (!isNewTagActive) {
                return;
            }

            var x = clamp(e.pageX - imgAnchor.left, 0, imgWidth);
            var y = clamp(e.pageY - imgAnchor.top, 0, imgHeight);

            $tagFrame.css({
                width: x - tagAnchor.left,
                height: y - tagAnchor.top
            });
        });

        $doc.on('mouseup', function() {
            if (!isNewTagActive) {
                return;
            }

            $tagFrame.css({
                width: clamp($tagFrame.css('width'), MIN_SIZE),
                height: clamp($tagFrame.css('height'), MIN_SIZE)
            });

            var newTagData = {
                PageId: null,
                ObjectTitle: null,
                Coordinates: null
            };

            syncTagSize($tagFrame, newTagData);
            tagsData.push(newTagData);
            var $tag = createTag(newTagData);

            isNewTagPending = false;
            isNewTagActive = false;
            $wrap.removeClass('new');
            $btn.removeClass('active');
            $tagFrame.remove();

            selectTag($tag);

            $tag.find('.media-tag-popup-form input').click();
        });
    }

    function positionTag($tag, tagData) {
        var coords = (tagData.Coordinates || '').split(';');
        if (coords.length < 4) {
            return;
        }

        $tag.css({
            left: Math.round(coords[0] * wrapWidth),
            top: Math.round(coords[1] * wrapHeight),
            width: Math.round(coords[2] * wrapWidth),
            height: Math.round(coords[3] * wrapHeight)
        });
    }

    function clickInside($elem, e) {
        return $elem.is(e.target) || $elem.has(e.target).length > 0;
    }

    function isGuid(value) {
        return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
    }

    function clamp(value, low, high) {
        if (typeof value === 'string') {
            value = parseInt(value);
        }
        if (value < low) return low;
        if (typeof high !== 'undefined' && value > high) return high;
        return value;
    }
})