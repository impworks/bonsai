$(function() {
    var $form = $('.page-editor');
    if ($form.length === 0) {
        return;
    }

    var $draftInfo = $('#editor-draft-info'),
        $discard = $('.cmd-discard-draft'),
        $preview = $('.cmd-preview'),
        $discardForm = $('#discard-draft-form');

    var isModified = false,
        elemSelector = 'input[name], textarea[name], select[name]';

    monitorChanges();
    setInterval(trySaveDraft, 5000);

    $discard.on('click', discardDraft);
    $preview.on('click', previewDraft);

    function trySaveDraft() {
        if (!isModified) {
            return;
        }

        saveDraft();
    }

    function saveDraft() {
        var state = getFormState();
        return $.ajax({
                url: '/admin/drafts/update',
                method: 'POST',
                data: state
            })
            .then(function (data) {
                isModified = false;
                var date = new Date(data.lastUpdateDate);
                $draftInfo.text('Черновик сохранен в ' + date.toLocaleTimeString() + '.');
            });
    }

    function getFormState() {
        var data = {};
        $form.find(elemSelector).each(function(idx, elem) {
            var $elem = $(elem);
            data[$elem.attr('name')] = $elem.val();
        });
        return data;
    }

    function monitorChanges() {
        $form.find(elemSelector).change(function () {
            isModified = true;
        });
    }

    function discardDraft() {
        var result = confirm('Все изменения для данной страницы будут сброшены.\n\nВы уверены?');
        if (!result) {
            return;
        }

        var $link = $(this);
        var origType = $link.data('page-type');
        if (origType) {
            $discardForm.find('input[name="type"]').val(origType);
        }

        $discardForm.submit();
    }

    function previewDraft() {
        saveDraft()
            .then(function() {
                var pageId = $discardForm.find('input[name="id"]').val();
                var url = '/admin/drafts/preview?id=' + pageId;
                window.open(url, 'bonsai_preview_' + pageId, 'menubar=no,toolbar=no,personalbar=no,resizable=yes,');
            });
    }
});