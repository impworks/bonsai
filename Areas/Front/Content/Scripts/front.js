$(function () {
    // media 
    $('body').magnificPopup({
        delegate: 'a.media-thumb-link',
        key: 'media-popup',
        type: 'ajax',
        gallery: {
            enabled: true,
            preload: [0, 2],
            tPrev: 'Назад',
            tNext: 'Вперед',
            tCounter: '<span class="mfp-counter">%curr% из %total%</span>'
        }
    });
});