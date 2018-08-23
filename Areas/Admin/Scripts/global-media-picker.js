$(function() {
    var $dialog = $('.modal-media-picker');
    if ($dialog.length === 0) {
        return;
    }

    window.pickMedia = pickMedia;

    function pickMedia(types, successHandler, failHandler) {
        $dialog.modal('show');

        var vue = new Vue({
            el: $('.modal.show.modal-media-picker')[0],
            data: {
                types: types || [],
                isTypeSelectionEnabled: !types || !types.length,
                query: '',
                media: [],
                picked: null
            },
            methods: {
                toggleType: function (t) {
                    var idx = this.types.indexOf(t);
                    if (idx === -1) {
                        this.types.push(t);
                    } else {
                        this.types.splice(idx, 1);
                    }
                },
                isTypeEnabled: function (t) {
                    return this.types.indexOf(t) !== -1;
                },
                pick: function (elem) {
                    this.picked = elem;
                    $dialog.modal('hide');
                }
            }
        });

        loadItems(vue);

        $dialog.on('hide.bs.modal', function () {
            if (vue.picked === null) {
                if (typeof failHandler === 'function') {
                    failHandler();
                }
            } else {
                if (typeof successHandler === 'function') {
                    successHandler(vue.picked);
                }
            }

            vue.$destroy();
            $dialog.off('hide.bs.modal');
        });
    }

    function loadItems(model) {
        var url = buildUrl(model);
        $.ajax(url).done(function (data) {
            if (data && data.length) {
                for (var i = 0; i < data.length; i++) {
                    model.media.push(data[i]);
                }
            }
        });
    }

    function buildUrl(model) {
        var parts = [];

        if (!!model.query) {
            parts.push('query=' + encodeURIComponent(model.query));
        }

        if (model.types && model.types.length) {
            for (var i = 0; i < model.types.length; i++) {
                parts.push('types=' + encodeURIComponent(model.types[i]));
            }
        }

        var url = '/admin/suggest/media';
        if (parts.length) {
            url += '?' + parts.join('&');
        }

        return url;
    }
});