$(function() {
    $('.md-editor').each(function (idx, elem) {
        var allowMedia = $(elem).data('md-pick-media');
        new SimpleMDE({
            element: elem,
            blockStyles: {
                bold: '**',
                italic: '__'
            },
            indentWithTabs: false,
            toolbar: buildStatusBar(allowMedia),
            spellChecker: false,
            status: false
        });
    });

    function buildStatusBar(allowMedia) {
        var bar = [
            'bold', 'italic', 'heading', '|',
            'unordered-list', 'ordered-list', '|',
            'link'
        ];

        if (allowMedia) {
            bar.push({
                name: 'image',
                className: 'fa fa-picture-o',
                title: 'Выбрать медиа-файл',
                action: function(editor) {
                    var cm = editor.codemirror;
                    pickMedia([], function(media) {
                        var text = '[[media:' + media.key + '|align:left|size:medium|Описание]]';
                        cm.replaceSelection(text);
                        var cur = cm.getCursor();
                        cm.setSelection({ line: cur.line, ch: cur.ch - 2 }, { line: cur.line, ch: cur.ch - 10 });
                        cm.focus();
                    });
                }
            });
        }

        return bar;
    }
});