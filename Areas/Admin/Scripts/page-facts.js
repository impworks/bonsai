$(function() {
    var $facts = $('#Facts');
    if ($facts.length === 0) {
        return;
    }

    var $title = $('#Title');
    var intrinsics = {
        name: function (data, key, value) {
            var names = data[key].Values;
            if (names.length > 0) {
                var latest = names[names.length - 1];
                value.FirstName = latest.FirstName;
                value.MiddleName = latest.MiddleName;
                if (latest.Duration && latest.Duration.length) {
                    var durParts = latest.Duration.split('-');
                    value.Duration = durParts[1] + '-';
                }
            } else {
                var title = $title.val();
                if (title && title.length) {
                    var titleParts = title.replace(/^\s+|\s+$|\s+(?=\s)/g, '').split(' ');
                    if (titleParts.length > 0) value.LastName = titleParts[0];
                    if (titleParts.length > 1) value.FirstName = titleParts[1];
                    if (titleParts.length > 2) value.MiddleName = titleParts[2];
                }
            }
        },
        gender: function (data, key, value) {
            var nameFact = data['Main.Name'];
            if (!nameFact || !nameFact.Values || !nameFact.Values.length) {
                return;
            }
            var middle = nameFact.Values[0].MiddleName;
            value.IsMale = /ич$/i.test(middle || '');
        },
        language: function (data, key, value) {
            if (data[key].Values.length === 0) {
                value.Name = 'Русский';
                value.Proficiency = 'Native';
            }
        }
    };

    Vue.component('date-picker', {
        template: '#date-picker-template',
        props: ['value', 'size', 'margin'],

        mounted: function() {
            var self = this;
            var $el = $(self.$el);
            var $d = setupDatepicker($el, true);

            $d.val(this.$props.value || '');

            $d.trigger('change')
              .on('change', function() {
                    self.$emit('input', this.value);
                });

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
            value: function (value) {
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
                set: function (value, intr) {
                    if (intrinsics[intr]) {
                        intrinsics[intr](this.data, this.def.key, value);
                    }
                    Vue.set(this.data, this.def.key, value);
                },
                add: function (value, intr) {
                    var key = this.def.key;
                    var v = 'Values';
                    if (typeof this.data[key] === 'undefined') {
                        this.set({});
                    }
                    if (typeof this.data[key][v] === 'undefined') {
                        Vue.set(this.data[key], v, []);
                    }
                    if (intrinsics[intr]) {
                        intrinsics[intr](this.data, key, value);
                    }
                    this.data[key][v].push(value);
                },
                remove: function(elem) {
                    var array = this.data[this.def.key]['Values'];
                    var itemId = array.indexOf(elem);
                    if (itemId !== -1) {
                        array.splice(itemId, 1);
                    }
                    if (array.length === 0) {
                        delete this.data[this.def.key];
                    }
                },
                unset: function () {
                    Vue.delete(this.data, this.def.key);
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
                    $facts.change();
                }
            }
        }
    });
});