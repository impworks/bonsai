$(function() {
    var $dialog = $('.media-picker');
    if ($dialog.length === 0) {
        return;
    }

    window.pickMedia = pickMedia;

    var vue = new Vue({
        el: $('.modal.media-picker .modal-content')[0],
        data: {
            types: [],
            isTypeSelectionEnabled: true,
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
            },
            search: function() {
                this.media.splice(0);
                loadItems(this);
            }
        }
    });

    function pickMedia(types, successHandler, failHandler) {
        $dialog.modal('show');

        resetForm(types);
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

        if (model.media && model.media.length > 0) {
            parts.push('offset=' + model.media.length);
        }

        var url = '/admin/suggest/media';
        if (parts.length) {
            url += '?' + parts.join('&');
        }

        return url;
    }

    function resetForm(types) {
        vue.types = types || [];
        vue.isTypeSelectionEnabled = !types || !types.length;
        vue.query = '';
        vue.media = [];
        vue.picked = null;
    }
});