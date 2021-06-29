$(function() {
    $('.md-editor').each(function (idx, elem) {
        var allowMedia = $(elem).data('md-pick-media');
        var editor = new EasyMDE({
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

        bar.push('|');

        bar.push({
            name: 'markdown',
            className: 'fa fa-question-circle',
            title: 'Справка по форматированию',
            action: function() {
                window.open('/admin/help/markdown', 'bonsai_help', 'menubar=no,toolbar=no,personalbar=no,resizable=yes,width=800,height=600');
            }
        });

        return bar;
    }
});