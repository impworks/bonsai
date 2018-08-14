$(function() {
    var $facts = $('#Facts');
    if ($facts.length === 0) {
        return;
    }

    Vue.component('date-picker', {
        template: '#DatePickerTemplate',
        props: ['value', 'size', 'margin'],

        mounted: function() {
            var self = this;
            var $el = $(self.$el);
            var $d = setupDatepicker($el);
            $d.trigger('change')
              .on('change', function() {
                    self.$emit('input', this.value);
                });

            var size = this.$attrs.size;
            var margin = this.$attrs.margin;
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
        template: '#DurationPickerTemplate',
        props: ['value', 'size', 'margin'],
        data: function () {
            var parts = (this.value || '').split('-');
            return {
                value: this.value,
                start: parts.length === 2 ? parts[0] : null,
                end: parts.length === 2 ? parts[1] : null
            };
        },
        methods: {
            refresh: function(start, end) {
                this.value = start || end ? start + '-' + end : null;
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
                add: function (key, value) {
                    if (typeof this.data[def.key] === 'undefined') {
                        this.set({});
                    }
                    if (typeof this.data[def.key][key] === 'undefined') {
                        Vue.set(this.data[def.key], key, []);
                    }
                    this.data[def.key][key].push(value);
                },
                unset: function () {
                    this.set(null);
                },
                empty: function () {
                    return this.data[this.def.key] == null;
                },
                emptyList: function(key) {
                    var root = this.data[this.def.key] || {};
                    var arr = root[key] || [];
                    return arr.length === 0;
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