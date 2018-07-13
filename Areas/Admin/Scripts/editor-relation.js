$(function() {
    if ($('#relation-editor-form').length === 0)
        return;

    var $sourceRow = $('.form-row[data-tier="source"]'),
        $sourceEditor = $sourceRow.find('input'),
        $typeRow = $('.form-row[data-tier="type"]'),
        $typeEditor = $typeRow.find('input'),
        $destRow = $('.form-row[data-tier="destination"]'),
        $destEditor = $destRow.find('input'),
        $durationRow = $('.form-row[data-tier="type"]'),
        $durationEditor = $durationRow.find('input'),
        $eventRow = $('.form-row[data-tier="type"]'),
        $eventEditor = $eventRow.find('input');
});