$(function () {
    var key = 'bonsai.preferences.view-tree-kind';

    var $view = $('.tree-view'),
        $iframe = $view.find('iframe'),
        $toggles = $view.find('.cmd-switch-tree'),
        $kindLabel = $view.find('.cmd-tree-kind-label'),
        $newWin = $view.find('.cmd-new-window');

    if($view.length === 0)
        return;

    $view.find('.cmd-fullscreen').click(function () {
        $iframe.fullScreen(true);
    });

    $toggles.click(function (e) {
        e.preventDefault();

        var $this = $(this);
        var kind = $this.data('kind');
        var label = $this.data('label') || $this.text().trim();

        $toggles.removeClass('active');
        $toggles.filter('[data-kind="' + kind + '"]').addClass('active');

        var url = $this.attr('href');
        $newWin.attr('href', url);
        $iframe.attr('src', url);
        $kindLabel.text(label);

        window.localStorage.setItem(key, kind);
    });

    var state = window.localStorage.getItem(key);
    if (state) {
        $toggles.filter('[data-kind="' + state + '"]')
                .first()
                .trigger('click');
    }
});