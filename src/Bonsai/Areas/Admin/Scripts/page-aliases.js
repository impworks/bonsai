$(function() {
    var $aliases = $('#Aliases');
    if ($aliases.length === 0) {
        return;
    }

    var aliases = JSON.parse($aliases.val() || '[]')
        .map(function(x) {
            return { value: x };
        });

    var editorApp = new Vue({
        el: '#editor-aliases',
        data: {
            aliases: aliases
        },
        computed: {
            canRemove: function() {
                return this.aliases.length > 1;
            }
        },
        methods: {
            removeAlias: function(index) {
                this.aliases.splice(index, 1);
                this.refresh();
            },
            addAlias: function() {
                this.aliases.push({ value: '' });
                this.refresh();

                this.$nextTick(function () {
                    var idx = this.aliases.length - 1;
                    var input = this.$refs.value[idx];
                    input.focus();
                });
            },
            refresh: function () {
                var values = aliases.map(function(x) { return x.value; });
                $aliases.val(JSON.stringify(values));
                $aliases.change();
            }
        }
    });
})