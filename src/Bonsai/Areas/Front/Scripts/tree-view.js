$(function () {
    var $view = $('.tree-view'),
        $iframe = $view.find('iframe');
    
    if($view.length === 0)
        return;
    
    // $iframe.on('load', function () {
    //     setTimeout(
    //         function() {
    //             var size = $iframe[0].contentWindow.document.documentElement.scrollHeight;
    //             $iframe.css('height', Math.min(size + 2, 1000) + 'px');
    //         },
    //         200
    //     );
    // });

    $view.find('.cmd-fullscreen').click(function () {
        $iframe.fullScreen(true);
    });
});