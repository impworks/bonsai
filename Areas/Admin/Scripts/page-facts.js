$(function() {
    var $facts = $('#Facts');
    if ($facts.length === 0) {
        return;
    }

    var $tpls = $("script[data-kind='fact-template']");
    $tpls.each(function (idx, elem) {
        var $elem = $(elem);

        Vue.component($elem.attr('id'), {
            props: ['data', 'def'],
            template: $elem.html(),
            methods: {
                set: function (value) {
                    Vue.set(this.data, this.def.key, value);
                },
                unset: function() {
                    this.set(null);
                },
                empty: function() {
                    return this.data[this.def.key] == null;
                }
            }
        });
    });

    var editor = new Vue({
        el: '#editor-facts',
        data: {
            data: JSON.parse($facts.val() || '{}')
        }
    });
});