$(function() {
    var $cm = $('.CodeMirror');
    if (!$cm.length)
        return;

    var isOpen = false;
    var query = '';
    var $popup = null;
    var results = [];
    var resultIdx = null;

    document.addEventListener('keydown',
        function(e) {
            if (isOpen) {
                if (e.key === 'Escape') {
                    closePopup();
                } else if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
                    e.preventDefault();
                    selectOffset(e.key === 'ArrowDown' ? 1 : -1);
                } else if (e.key === 'Backspace') {
                    // todo: ctrl + backspace?
                    if (query)
                        setQuery(query.substr(0, query.length - 1));
                    else
                        closePopup();
                } else if (e.key === 'Enter') {
                    if (resultIdx !== null) {
                        e.preventDefault();
                        pick();
                    }
                }

                // todo: left-right?
            } else {
                if (e.key === '@')
                    openPopup();
            }
        },
        true
    );

    document.addEventListener('keypress',
        function(e) {
            if (!isOpen)
                return;
            if (query === '' && e.key === '@')
                return;

            setQuery(query + e.key);
        },
        true
    );

    /**
     * Displays the popup.
     */
    function openPopup() {
        if (isOpen)
            return;

        isOpen = true;
        var pos = $cm.find('.CodeMirror-cursor').offset();
        $popup = $('<div>').addClass('eac-popup').css({ top: pos.top + 25, left: pos.left });
        $popup.appendTo($('body'));

        refreshPopup();
    }

    /**
     * Hides the popup.
     */
    function closePopup() {
        if (!isOpen)
            return;

        $popup.remove();
        $popup = null;

        isOpen = false;
        query = '';
        results = [];
        resultIdx = null;
    }

    /**
     * Updates the query to a new value.
     * @param {string} q
     */
    function setQuery(q) {
        if (query === q)
            return;

        query = q;

        // todo: throttle!
        $.ajax({
            url: '/admin/pick/pages',
            method: 'GET',
            data: {
                count: 5,
                query: query
            }
        }).then(data => {
            console.log('found for ' + query + ':', data);
            results = data;
            if (results && results.length)
                resultIdx = 0;
            refreshPopup();
        });
    }

    /**
     * Renders the popup with updated data.
     */
    function refreshPopup() {
        if (!isOpen)
            return;

        $popup.empty();

        if (results.length > 0) {
            for (var i = 0; i < results.length; i++) {
                $popup.append(renderElem(i));
            }
        } else {
            $popup.append(renderEmptyMsg());
        }

        function renderElem(index) {
            var isActive = index === resultIdx;
            var text = results[index].title;

            var $elem = $('<div>').addClass('eac-popup-item clickable')
                                  .text(text);

            if (isActive)
                $elem.addClass('active');

            $elem.on('mouseover', function () { $(this).addClass('active'); });
            $elem.on('mouseout', function () { if (!isActive) $(this).removeClass('active'); });

            // todo: click to select!

            return $elem;
        }

        function renderEmptyMsg() {
            return $('<div>').addClass('eac-popup-empty').text(query ? 'Ничего не найдено' : 'Введите запрос для поиска');
        }
    }

    /**
     * Selects the element by offset (1 for next, -1 for previous).
     * @param {number} offset
     */
    function selectOffset(offset) {
        selectIndex(resultIdx + offset);
    }

    /**
     * Selects the element as current, refreshing the popup.
     * @param {number} idx
     */
    function selectIndex(idx) {
        if (results.length === 0) {
            resultIdx = null;
            return;
        }

        resultIdx = idx;

        if (resultIdx >= results.length) {
            resultIdx = result.length - 1;
            return;
        }

        if (resultIdx < 0) {
            resultIdx = 0;
            return;
        }

        refreshPopup();
    }

    /**
     * Inserts the selection.
     */
    function pick(idx = null) {
        var result = results[idx || resultIdx];
        var cm = $cm.get(0).CodeMirror;
        var cursor = cm.doc.getCursor();
        cm.replaceRange(
            '[[' + result.title + '|' + result.title + ']]',
            { line: cursor.line, ch: cursor.ch - query.length - 1 },
            cursor
        );
        closePopup();
    }
});