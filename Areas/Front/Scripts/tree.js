$(function () {
    var CARD_WIDTH = 300,
        CARD_HEIGHT = 100,
        SPACING = 30;

    var $trees = $('.tree');
    if ($trees.length === 0) {
        return;
    }

    Vue.component('tree-card', {
        template: '#tree-card-template',
        props: ['value']
    });

    $trees.each(function () {
        var $tree = $(this);
        var $wrap = $tree.find('.tree-wrapper');
        var $data = $($wrap.data('src'));
        if ($data.length === 0) {
            return;
        }

        $tree.find('.cmd-fullscreen').click(function () {
            var $btn = $(this);
            var newState = !$(document).fullScreen();
            $btn.toggleClass('active', newState);
            $tree.find('.tree-wrapper').fullScreen(newState);
        });

        var treeData = JSON.parse($data[0].innerText);
        var elkJson = generateElkJson(treeData);
        var elk = new ELK();

        elk.layout(elkJson)
            .then(function (tree) { renderTree($wrap, tree); })
            .catch(console.error);
    });

    function generateElkJson(data) {
        // converts Bonsai-style JSON to ELK-style
        var nodes = [];
        var edges = [];

        processPersons();
        processRelations();

        return {
            id: "root",
            layoutOptions: {
                'elk.algorithm': 'layered',
                'elk.direction': 'DOWN',
                'elk.edgeRouting': 'ORTHOGONAL',
                'elk.layered.spacing.edgeEdgeBetweenLayers': SPACING,
                'elk.layered.spacing.edgeNodeBetweenLayers': SPACING,
                'elk.spacing.nodeNode': SPACING,
                'elk.layered.nodePlacement.favorStraightEdges': false
            },
            children: nodes,
            edges: edges
        };

        function processPersons() {
            // registers person cards
            for (var i = 0; i < data.Persons.length; i++) {
                var person = data.Persons[i];
                nodes.push({
                    id: person.Id,
                    label: person.Name,
                    width: CARD_WIDTH,
                    height: CARD_HEIGHT,
                    info: person,
                    layoutOptions: {
                        'elk.portConstraints': 'FIXED_SIDE'
                    },
                    ports: [
                        {
                            id: person.Id + ":n",
                            layoutOptions: {
                                'elk.port.side': 'NORTH'
                            }
                        },
                        {
                            id: person.Id + ":s",
                            layoutOptions: {
                                'elk.port.side': 'SOUTH'
                            }
                        }
                    ]
                });

                if (person.Parents !== null) {
                    edges.push({
                        id: person.Id + ':' + person.Parents,
                        sources: [person.Parents + ':s'],
                        targets: [person.Id + ':n'],
                        info: {
                            fakeTarget: false
                        }
                    });
                }
            }
        }

        function processRelations() {
            // registers marriage pseudo-nodes
            for (var i = 0; i < data.Relations.length; i++) {
                var rel = data.Relations[i];
                nodes.push({
                    id: rel.Id,
                    width: 1,
                    height: 1,
                    layoutOptions: {
                        'elk.portConstraints': 'FIXED_SIDE'
                    },
                    ports: [
                        {
                            id: rel.Id + ":n",
                            layoutOptions: {
                                'elk.port.side': 'NORTH'
                            }
                        },
                        {
                            id: rel.Id + ":s",
                            layoutOptions: {
                                'elk.port.side': 'SOUTH'
                            }
                        }
                    ]
                });

                edges.push({
                    id: rel.Id + ':' + rel.From,
                    sources: [rel.From],
                    targets: [rel.Id + ':n'],
                    info: {
                        fakeTarget: true
                    }
                });

                edges.push({
                    id: rel.Id + ':' + rel.To,
                    sources: [rel.To],
                    targets: [rel.Id + ':n'],
                    info: {
                        fakeTarget: true
                    }
                });
            }
        }
    }

    function renderTree($wrap, tree) {
        // displays the tree
        var persons = convertPersons(tree);
        var edges = convertEdges(tree);
        var vue = new Vue({
            el: $wrap[0],
            data: {
                persons: persons,
                edges: edges,
                width: tree.width,
                height: tree.height
            }
        });
    }

    function convertPersons(tree) {
        // returns the list of cards to render
        return tree.children.filter(function (x) { return x.info != null; });
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
            .filter(function (x) { return x.info == null; })
            .map(function (x) { return x.id });

        var result = {};
        for (var i = 0; i < relKeys.length; i++) {
            var key = relKeys[i];
            result[key + ':n'] = tree.edges.some(function (x) { return x.sources[0] === key + ':s' });
        }

        return result;
    }
});