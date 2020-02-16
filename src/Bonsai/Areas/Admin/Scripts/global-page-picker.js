function setupPagePicker($elem, opts) {
    opts = opts || {};
    var multiple = $elem.prop('multiple');

    function getUrl(query) {
        var url = '/admin/suggest/pages?query=' + encodeURIComponent(query);
        (opts.types || []).forEach(function (t) { url += '&types=' + encodeURIComponent(t); });
        return url;
    }

    $elem.selectize({
        create: opts.create || false,
        maxOptions: 50,
        maxItems: multiple ? null : 1,
        openOnFocus: true,
        valueField: 'id',
        labelField: 'title',
        sortField: [{ field: 'index', direction: 'asc' }],
        searchField: 'title',
        placeholder: opts.placeholder || 'Страница или имя',
        preload: true,
        onChange: opts.onChange,
        load: function (query, callback) {
            var self = this;
            var url = (opts.urlFactory || getUrl)(query);

            $.ajax(url)
                .done(function (data) {
                    // hack for maintaining relevance order
                    for (var i = 0; i < data.length; i++)
                        data['index'] = i;

                    // hack for fuzzy search:
                    // cache values returned from server
                    var cache = self.lastLoaded || (self.lastLoaded = {});
                    cache[query] = data;
                    self.lastQuery = null;
                    callback(data);
                });
        },
        score: function (query) {
            var self = this;
            var def = this.getScoreFunction(query);
            return function (item) {
                // hack for fuzzy search:
                // bump score for items that have been returned from server for this query
                var hasItem = self.lastLoaded
                    && self.lastLoaded[query]
                    && self.lastLoaded[query].some(function (x) { return x.id === item.id; });
                return def(item) && hasItem ? 1 : 0;
            };
        },
        render: {
            option_create: function (data, escape) {
                return '<div class="create">' + escape(data.input) + ' <i>(без ссылки)</i></div>';
            }
        }
    });
}