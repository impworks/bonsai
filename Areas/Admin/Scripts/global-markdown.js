$(function() {
    $('.md-editor').each(function (idx, elem) {
        var allowMedia = $(elem).data('md-pick-media');
        var editor = new SimpleMDE({
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

        editor.codemirror.on('change', function () {
            editor.codemirror.save();
            $(elem).change();
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

        bar.push({
            name: 'page',
            className: 'fa fa-file',
            title: 'Выбрать страницу',
            action: function (editor) {
                var cm = editor.codemirror;
                pickPage([], function (page) {
                    var text = '[[' + page.title + '|' + page.title + ']]';
                    cm.replaceSelection(text);
                    var cur = cm.getCursor();
                    cm.setSelection({ line: cur.line, ch: cur.ch - 2 }, { line: cur.line, ch: cur.ch - 2 - page.title.length });
                    cm.focus();
                });
            }
        });

        return bar;
    }
});