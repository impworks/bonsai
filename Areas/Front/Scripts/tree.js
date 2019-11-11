$(function () {
    var $trees = $('.tree');
    if ($trees.length === 0) {
        return;
    }

    Vue.component('tree-card', {
        template: '#tree-card-template',
        props: ['value', 'active']
    });

    $trees.each(function () {
        var $tree = $(this);
        var $wrap = $tree.find('.tree-wrapper');
        var key = $wrap.data('key');

        requestTreeInfo($wrap, key, 0);

        $tree.find('.cmd-fullscreen').click(function () {
            var $btn = $(this);
            var newState = !$(document).fullScreen();
            $btn.toggleClass('active', newState);
            $tree.find('.tree-wrapper').fullScreen(newState);
        });
    });

    function requestTreeInfo($wrap, key, count) {
        if (count > 10) {
            var $tree = $wrap.closest('.tree');
            $tree.find('.tree-preloader').remove();
            $tree.find('.tree-error').show();
            return;
        }

        var url = '/util/tree/' + encodeURIComponent(key);
        $.ajax(url)
            .then(function(data) {
                if (data && data.content) {
                    renderTree($wrap, data);
                    return;
                }

                setTimeout(
                    function () {
                        requestTreeInfo($wrap, key, count + 1);
                    },
                    5000
                );
            });
    }

    function renderTree($wrap, treeInfo) {
        // displays the tree
        var tree = treeInfo.content;
        var rootId = treeInfo.rootId;
        var persons = convertPersons(tree);
        var edges = convertEdges(tree);
        var vue = new Vue({
            el: $wrap[0],
            data: {
                persons: persons,
                edges: edges,
                width: tree.width,
                height: tree.height,
                root: rootId
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

    function convertEdges(tree) {
        // returns the SVG-friendly list of edges
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
                // +1 to account for a pseudo-node's height (avoids gaps)
                points.push(e.x, e.y + 1);
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

        var vw = $view.width(),
            vh = $view.height(),
            cw = $card.width(),
            ch = $card.height(),
            cp = $card.position();

        var dx = (vw - cw) / 2,
            dy = (vh - ch) / 2;

        $view.scrollLeft(Math.max(0, cp.left - dx))
            .scrollTop(Math.max(0, cp.top - dy));
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
            var dx = newOrigin.x - origin.x;
            var dy = newOrigin.y - origin.y;
            $view[0].scrollLeft -= dx;
            $view[0].scrollTop -= dy;
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
    }
});
