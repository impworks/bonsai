$(function () {
    var $trees = $('.tree');
    if ($trees.length === 0) {
        return;
    }

    Vue.component('tree-card', {
        template: '#tree-card-template',
        props: ['value', 'active', 'compact'],
        computed: {
            fullName: function () {
                var info = this.value.info;
                return info.MaidenName
                    ? info.Name + ' (' + info.MaidenName + ')'
                    : info.Name;
            },
            displayName: function () {
                // a compact card has no room for the whole name, so it shows the
                // surname only and keeps the full one in the tooltip
                return this.compact
                    ? getSurname(this.value.info.Name)
                    : this.value.info.Name;
            },
            displayInitials: function () {
                // the initials are rendered separately, because they must not be
                // broken apart by a line wrap
                return this.compact
                    ? getInitials(this.value.info.Name)
                    : null;
            },
            dates: function () {
                // a compact card only has room for the years, and it drops them
                // altogether when nothing is known
                var info = this.value.info;
                return this.compact
                    ? formatDates(getYear(info.Birth), getYear(info.Death))
                    : formatDates(info.Birth, info.Death);
            },
            tooltip: function () {
                // the compact card is abbreviated, so the tooltip carries the
                // full name and the full dates
                if (!this.compact) {
                    return null;
                }

                var info = this.value.info;
                var dates = formatDates(info.Birth, info.Death);
                return dates
                    ? this.fullName + '\n' + dates
                    : this.fullName;
            }
        }
    });

    $trees.each(function () {
        var $tree = $(this);
        var $wrap = $tree.find('.tree-wrapper');

        var key = $wrap.data('key');
        var kind = $wrap.data('kind');
        var url = '/util/tree/' + encodeURIComponent(key) + '?kind=' + encodeURIComponent(kind);

        var view = {
            direction: $wrap.data('direction'),
            compact: $wrap.data('view') === 'Compact'
        };

        requestTreeInfo($wrap, url, view, 0);
    });

    function requestTreeInfo($wrap, url, view, retryCount) {
        if (retryCount > 10) {
            var $tree = $wrap.closest('.tree');
            $tree.find('.tree-preloader').remove();
            $tree.find('.tree-error').show();
            return;
        }

        $.ajax(url)
            .then(function(data) {
                if (data && data.content) {
                    renderTree($wrap, data, view);
                    return;
                }

                setTimeout(
                    function () {
                        requestTreeInfo($wrap, url, view, retryCount + 1);
                    },
                    5000
                );
            }, function () {
                requestTreeInfo($wrap, url, view, 11);
            });
    }

    function renderTree($wrap, treeInfo, view) {
        // displays the tree
        var tree = treeInfo.content;
        var rootId = treeInfo.rootId;
        var persons = convertPersons(tree);
        var edges = convertEdges(tree, view.direction);
        var vue = new Vue({
            el: $wrap[0],
            data: {
                persons: persons,
                edges: edges,
                width: tree.width,
                height: tree.height,
                root: rootId,
                compact: view.compact
            },
            mounted: function () {
                var $view = $(this.$el);
                scrollIntoView($view, rootId);
                enableDrag($view);
                $view.closest('.tree').find('.tree-preloader').remove();
            }
        });
    }

    function convertPersons(tree) {
        // returns the list of cards to render
        return tree.children.filter(function (x) { return !!x.info; });
    }

    function splitName(name) {
        return (name || '').split(' ').filter(function (x) { return x.length > 0; });
    }

    function getSurname(name) {
        // "Горбунов Дмитрий Владимирович" -> "Горбунов"
        var parts = splitName(name);
        return parts.length < 2 ? name : parts[0];
    }

    function getInitials(name) {
        // shortens everything after the surname to an initial:
        // "Горбунов Дмитрий Владимирович" -> "Д. В."
        var parts = splitName(name);
        if (parts.length < 2) {
            return null;
        }

        return parts.slice(1)
                    .map(function (x) { return x.charAt(0).toUpperCase() + '.'; })
                    .join(' ');
    }

    function getYear(date) {
        // returns the year component of a short date ("12/03/1980" -> "1980"),
        // or nothing at all when the year is unknown
        if (!date) {
            return null;
        }

        var year = date.split('/').pop();
        return year.indexOf('?') === 0 ? null : year;
    }

    function formatDates(birth, death) {
        // renders the lifespan; either of the dates may be missing
        if (birth && death) {
            return birth + ' — ' + death;
        }

        if (death) {
            return '— ' + death;
        }

        return birth || null;
    }

    function convertEdges(tree, direction) {
        // returns the SVG-friendly list of edges
        // a wire ends on the border of a card, so the last point is nudged one
        // pixel into it - which is upwards when the tree grows from the bottom up
        var nudge = direction === 'BottomToTop' ? -1 : 1;
        var hasChildren = detectChildren(tree);
        var result = [];
        for (var idx = 0; idx < tree.edges.length; idx++) {
            var edgeInfo = tree.edges[idx];
            var edge = edgeInfo.sections[0];
            var s = edge.startPoint;
            var e = edge.endPoint;
            var points = [s.x, s.y];
            if (edge.bendPoints && edge.bendPoints.length) {
                for (var idx2 = 0; idx2 < edge.bendPoints.length; idx2++) {
                    var b = edge.bendPoints[idx2];
                    points.push(b.x, b.y);
                }
            }
            if (!edgeInfo.info.fakeTarget || hasChildren[edgeInfo.targets[0]]) {
                // omit the last segment if the marriage has no children (avoids dangling connector)
                // the nudge accounts for a pseudo-node's height (avoids gaps)
                points.push(e.x, e.y + nudge);
            }
            result.push({
                // +0.5 for crispy clear nodes
                points: points.map(function(x) { return Math.round(x) + 0.5; }).join(' ')
            });
        }
        return result;
    }

    function detectChildren(tree) {
        // checks which relations have children
        var relKeys = tree.children
            .filter(function (x) { return !x.info; })
            .map(function (x) { return x.id; });

        var result = {};
        for (var i = 0; i < relKeys.length; i++) {
            var key = relKeys[i];
            result[key + ':n'] = tree.edges.some(function (x) { return x.sources[0] === key + ':s'; });
        }

        return result;
    }

    function scrollIntoView($view, id) {
        // adjusts the scroll so that the element is in the center of the window
        var $card = $view.find(".tree-card-wrapper[data-id='" + id + "']");
        if ($card.length === 0) {
            return;
        }
        
        var pos = $card[0].getBoundingClientRect();
        var x = pos.left + (pos.right - pos.left - window.innerWidth) / 2;
        var y = pos.top + (pos.bottom - pos.top - window.innerHeight) / 2;
        window.scroll(x, y);
    }

    function enableDrag($view) {
        // allows scrolling by drag
        var origin = null;
        var isDragged = false;
        var preventClick = false;

        var mouseUp = function() {
            if (isDragged) {
                preventClick = true;
            }
            origin = null;
            isDragged = false;
            $view.removeClass('dragged');
            $(document).off('mouseup', mouseUp);
            $(document).off('mousemove', mouseMove);
        };

        var mouseMove = function(e) {
            if (!isDragged) {
                isDragged = true;
                $view.addClass('dragged');
            }
            var newOrigin = { x: e.clientX, y: e.clientY };
            var dx = origin.x - newOrigin.x;
            var dy = origin.y - newOrigin.y;
            scrollBy(dx, dy)
            origin = newOrigin;
        };

        $view.on('mousedown', function(e) {
            origin = { x: e.clientX, y: e.clientY };
            $(document).on('mouseup', mouseUp);
            $(document).on('mousemove', mouseMove);
        });

        $view.on('click', function(e) {
            if (preventClick) {
                e.preventDefault();
                preventClick = false;
            }
        });
        
        function scrollBy(dx, dy) {
            var d = document, r = d.documentElement, b = d.body;
            var x = r.scrollLeft || b.scrollLeft || 0,
                y = r.scrollTop || b.scrollTop || 0;
            window.scroll(x + dx, y + dy);
        }
    }
});
