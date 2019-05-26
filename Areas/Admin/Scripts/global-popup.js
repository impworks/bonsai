$(function() {
    $('body').on('click', '.cmd-show-popup', function (e) {
        e.preventDefault();
        var $this = $(this);
        var url = $this.data('popup-url');
        var opts = $this.data('popup-options') || 'menubar=no,toolbar=no,personalbar=no,resizable=yes,width=800,height=600';
        window.open(url, 'bonsai_help', opts);
    });
})