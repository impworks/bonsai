$(function() {
    var $facts = $('#Facts');
    if ($facts.length === 0) {
        return;
    }

    Vue.component('date-picker', {
        template: '#date-picker-template',
        props: ['value', 'size', 'margin'],

        mounted: function() {
            var self = this;
            var $el = $(self.$el);
            var $d = setupDatepicker($el);
            $d.trigger('change')
              .on('change', function() {
                    self.$emit('input', this.value);
                });

            $d.val(this.$props.value || '');

            var size = this.$props.size;
            var margin = this.$props.margin;
            if (typeof size !== 'undefined') {
                $el.addClass('form-control-' + size);
                $el.closest('.input-group').addClass('input-group-' + size);
            }

            if (typeof margin !== 'undefined') {
                $el.closest('.input-group').addClass('mr-' + margin);
            }
        },

        watch: {
            value: function(value) {
                $(this.$el).val(value);
            }
        },

        destroyed: function() {
            $(this.$el).datepicker('destroy');
        }
    });

    Vue.component('duration-picker', {
        template: '#duration-picker-template',
        props: ['value', 'size', 'margin'],
        data: function () {
            var parts = (this.$props.value || '').split('-');
            return {
                start: parts.length === 2 ? parts[0] : null,
                end: parts.length === 2 ? parts[1] : null
            };
        },
        methods: {
            refresh: function(start, end) {
                this.value = start || end ? (start || '') + '-' + (end || '') : null;
                this.$emit('input', this.value);
            }
        },
        watch: {
            start: function(val) {
                this.refresh(val, this.end);
            },
            end: function(val) {
                this.refresh(this.start, val);
            }
        }
    });

    var $tpls = $("script[data-kind='fact-template']");
    $tpls.each(function (idx, elem) {
        var id = $(elem).attr('id');

        Vue.component(id, {
            props: ['data', 'def'],
            template: '#' + id,
            methods: {
                set: function (value) {
                    Vue.set(this.data, this.def.key, value);
                },
                add: function (value) {
                    if (typeof this.data[def.key] === 'undefined') {
                        this.set({});
                    }
                    if (typeof this.data[def.key]['Values'] === 'undefined') {
                        Vue.set(this.data[def.key], 'Values', []);
                    }
                    this.data[def.key][key].push(value);
                },
                unset: function () {
                    this.set(null);
                },
                empty: function () {
                    return this.data[this.def.key] == null;
                },
                emptyList: function() {
                    return this.getList().length === 0;
                },
                getList: function() {
                    var root = this.data[this.def.key] || {};
                    return root['Values'] || [];
                }
            }
        });
    });

    var globalData = JSON.parse($facts.val() || '{}');
    var editor = new Vue({
        el: '#editor-facts',
        data: {
            data: globalData
        },
        watch: {
            data: {
                deep: true,
                handler: function() {
                    $facts.val(JSON.stringify(globalData));
                }
            }
        }
    });
});