$(function() {
    var $cm = $('.CodeMirror');
    if (!$cm.length)
        return;

    var isOpen = false;
    var query = '';
    var $popup = null;

    document.addEventListener('keydown',
        function(e) {
            if (isOpen) {
                if (e.key === 'Escape')
                    close();
                else if (e.key === 'ArrowDown' || e.key == 'ArrowUp') {
                    e.stopPropagation();
                    e.stopImmediatePropagation();
                    e.preventDefault();
                    console.log('fired!');
                } else if (e.key === 'Backspace') {
                    if (query)
                        setQuery(query.substr(0, query.length - 1));
                    else
                        close();
                }
            } else {
                if (e.key === '@')
                    open();
            }
        },
        true);

    document.addEventListener('keypress', function (e) {
        if (!isOpen)
            return;
        if (query === '' && e.key === '@')
            return;

        setQuery(query + e.key);
    }, true);

    function open() {
        if (isOpen)
            return;

        isOpen = true;
        var pos = $cm.find('.CodeMirror-cursor').offset();
        $popup = $('<div>').attr({ class: 'xhover' }).css({top: pos.top + 25, left: pos.left });
        $popup.appendTo($('body'));
    }

    function close() {
        if (!isOpen)
            return;

        isOpen = false;
        $popup.remove();
        $popup = null;
    }

    function setQuery(q) {
        query = q;
        $popup.text(q);
    }
});