$(function () {
    var $b = $('body');

    function highlightAll(state) {
        $('.media-wrapper .photo-tag')
            .tooltip('hide')
            .css('opacity', state ? 0.4 : 0);
    }

    function highlightTag($tag) {
        $('.media-wrapper .photo-tag')
            .css('opacity', 0.2);

        $tag.css('opacity', 0.6)
            .tooltip('show');
    }

    // display media wrapper links on image hover
    $b.on('mouseover', '.media-wrapper img.media-photo', function () {
        highlightAll(true);
    });

    $b.on('mouseout', '.media-wrapper img.media-photo', function () {
        highlightAll(false);
    });

    $b.on('mouseover', '.media-wrapper .photo-tag', function () {
        highlightTag($(this));
    });

    $b.on('mouseover', '.media-tags .media-tag-link', function () {
        var id = $(this).data('tag-id');
        var $tag = $('.media-wrapper .photo-tag[data-tag-id="' + id + '"]');
        highlightTag($tag);
    });

    $b.on('mouseout', '.media-tags .media-tag-link', function () {
        highlightAll(false);
    });
});