function throttle(wait, func) {
    var ctx, args, rtn, timeoutId; // caching
    var last = 0;

    return function() {
        ctx = this;
        args = arguments;
        var delta = new Date() - last;
        if (!timeoutId) {
            if (delta >= wait)
                call();
            else
                timeoutId = setTimeout(call, wait - delta);
        }
        return rtn;
    };

    function call() {
        timeoutId = 0;
        last = +new Date();
        rtn = func.apply(ctx, args);
        ctx = null;
        args = null;
    }
}