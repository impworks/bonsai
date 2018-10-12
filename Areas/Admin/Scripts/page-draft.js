$(function() {
    var $form = $('.page-editor');
    if ($form.length === 0) {
        return;
    }

    var $draftInfo = $('#editor-draft-info'),
        $discard = $('.cmd-discard-draft');

    var isModified = false,
        elemSelector = 'input[name], textarea[name], select[name]';

    monitorChanges();
    setInterval(saveDraft, 5000);

    $discard.on('click', discardDraft);

    function saveDraft() {
        if (!isModified) {
            return;
        }

        var state = getFormState();
        $.ajax({
                url: '/admin/drafts/update',
                method: 'POST',
                data: state
            })
            .done(function (data) {
                var date = new Date(data.lastUpdateDate);
                $draftInfo.text('Черновик сохранен в ' + date.toLocaleTimeString() + '.');
            });

        isModified = false;
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

        $('#discard-draft-form').submit();
    }
});