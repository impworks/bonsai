$(function () {
    var key = 'bonsai.preferences.view-tree-kind';

    var $view = $('.tree-view'),
        $iframe = $view.find('iframe'),
        $toggles = $view.find('.cmd-switch-tree'),
        $newWin = $view.find('.cmd-new-window');
    
    if($view.length === 0)
        return;

    $view.find('.cmd-fullscreen').click(function () {
        $iframe.fullScreen(true);
    });

    $toggles.click(function (e) {
        e.preventDefault();

        var $this = $(this);

        $toggles.removeClass('active');
        $this.addClass('active');

        var url = $this.attr('href');
        $newWin.attr('href', url);
        $iframe.attr('src', url);

        window.localStorage.setItem(key, $this.data('kind'));
    });

    var state = window.localStorage.getItem(key);
    if (state) {
        $toggles.filter('[data-kind="' + state + '"]')
                .trigger('click');
    }
});