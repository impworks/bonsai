$(function() {
    var $facts = $('#Facts');
    if ($facts.length === 0) {
        return;
    }

    Vue.component('datepicker', {
        template: '#DatePickerTemplate',
        props: ['value'],

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
                    this.data[def.key][key].push(value);
                },
                unset: function () {
                    this.set(null);
                },
                empty: function () {
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