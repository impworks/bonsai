$(function () {
    function updateHandle($el, value) {
        $el.text(value + '');
    }

    var $handle = null;
    $('input[type="range"]').rangeslider({
        polyfill: false,
        onInit: function() {
            $handle = $('.rangeslider__handle', this.$range);
            updateHandle($handle, this.value);
        },
        onSlide: function() {
            updateHandle($handle, this.value);
        }
    });
});