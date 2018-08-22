$(function() {
    $('.md-editor').each(function(idx, elem) {
        new SimpleMDE({
            element: elem,
            blockStyles: {
                bold: '**',
                italic: '__'
            },
            indentWithTabs: false,
            hideIcons: ['preview', 'quote', 'side-by-side', 'image', 'guide'],
            spellChecker: false,
            status: false
        });
    });
});