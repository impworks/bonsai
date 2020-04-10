$(function() {
    var $toggle = $('.user-popover-toggle');
    var $target = $('.user-image');
    
    $target.popover({
        container: 'body',
        content: $('#user-popover-contents').html(),
        html: true,
        offset: '0, 4',
        placement: 'bottom',
        trigger: 'manual',
        template: '<div class="popover" role="tooltip"><div class="arrow"></div><div class="popover-body user-popover-body"></div></div>'
    });

    $toggle.click(function () {
        $target.popover('show');
    });

    $('body').on('click', function (e) {
        function notIn($elem) {
            return !$elem.is(e.target) && $elem.has(e.target).length === 0;
        }
        if (notIn($target) && notIn($toggle)) {
            $target.popover('hide');
        }
    });
})