(function () {
    window.pickMedia = createPicker({
        dialogSelector: '.media-picker',
        contentSelector: '.modal.media-picker .modal-content',
        url: '/admin/pick/media',
        failMsg: 'Не удалось загрузить медиа-файлы'
    });

    window.pickPage = createPicker({
        dialogSelector: '.page-picker',
        contentSelector: '.modal.page-picker .modal-content',
        url: '/admin/pick/pages',
        failMsg: 'Не удалось загрузить страницы'
    });

    function createPicker(options) {
        var $dialog = $(options.dialogSelector);
        if ($dialog.length === 0) {
            return undefined;
        }

        var $component = $(options.contentSelector);
        var isLoading = false;

        var vue = new Vue({
            el: $component[0],
            data: {
                types: [],
                isTypeSelectionEnabled: true,
                query: '',
                items: [],
                picked: null,
                isScrollEnd: false
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
                search: function () {
                    loadItems(this, true);
                }
            }
        });

        return function(types, successHandler, failHandler) {
            $dialog.modal('show');

            resetFilter(types);
            loadItems(vue, true);

            setupScrolling(vue);

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

        function loadItems(model, clear) {
            isLoading = true;
            if (clear) {
                model.items.splice(0);
            }

            var url = buildUrl(model);
            $.ajax(url).done(function(data) {
                    if (data && data.length) {
                        for (var i = 0; i < data.length; i++) {
                            model.items.push(data[i]);
                        }
                    }

                    model.isScrollEnd = data.length === 0;
                })
                .fail(function() {
                    toastr.error(options.failMsg);
                })
                .always(function() {
                    isLoading = false;
                });
        }

        function buildUrl(model) {
            var parts = [];

            if (model.query) {
                parts.push('query=' + encodeURIComponent(model.query));
            }

            if (model.types && model.types.length) {
                for (var i = 0; i < model.types.length; i++) {
                    parts.push('types=' + encodeURIComponent(model.types[i]));
                }
            }

            if (model.items && model.items.length > 0) {
                parts.push('offset=' + model.items.length);
            }

            var url = options.url;
            if (parts.length) {
                url += '?' + parts.join('&');
            }

            return url;
        }

        function resetFilter(types) {
            vue.types = types || [];
            vue.isTypeSelectionEnabled = !types || !types.length;
            vue.query = '';
            vue.items = [];
            vue.picked = null;
        }

        function setupScrolling(model) {
            var $scroll = $('.modal.show .scrollable');
            var $scrollEnd = $scroll.find('.scroll-end');
            var obs = new IntersectionObserver(
                function (entries) {
                    var entry = entries[0];
                    if (entry.intersectionRatio >= 0.5 && !isLoading && !model.isScrollEnd) {
                        loadItems(model, false);
                    }
                },
                {
                    root: $scroll[0],
                    threshold: 0.5
                }
            );
            obs.observe($scrollEnd[0]);
        }
    }
})();