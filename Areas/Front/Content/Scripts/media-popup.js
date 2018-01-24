$(function () {
    var prefix = 'media-';
    var selector = 'a.media-thumb-link';

    // sets the current media hash depending on an item
    function updateHash(elem) {
        if (elem == null) {
            history.pushState(
                "",
                document.title,
                window.location.pathname + window.location.search
            );
            return;
        }

        var id = elem.data('media');
        window.location.hash = prefix + id;
    }

    // enables media popups for thumbnails
    var allLinks = $('body').magnificPopup({
        delegate: selector,
        key: 'media-popup',
        type: 'ajax',
        tClose: 'Закрыть',
        tLoading: 'Загрузка...',
        ajax: {
            tError: 'Не удалось загрузить данные.'
        },
        gallery: {
            enabled: true,
            preload: [0, 2],
            tPrev: 'Назад',
            tNext: 'Вперед',
            tCounter: '<span class="mfp-counter">%curr% из %total%</span>'
        },
        callbacks: {
            open: function() {
                updateHash(this.currItem.el);
            },
            close: function () {
                updateHash(null);
            },
            change: function () {
                updateHash(this.currItem.el);
            },
        }
    });

    // opens the media popup if the URL contains a hash
    var hash = window.location.hash;
    if (hash != null && hash.substr(1, prefix.length) === prefix) {
        var media = hash.substr(prefix.length + 1);
        var elem = $(selector + '[data-media="' + media + '"]');
        var pos = allLinks.find(selector).index(elem);
        if (pos !== -1) {
            allLinks.magnificPopup('open', pos);
        }
    }
});