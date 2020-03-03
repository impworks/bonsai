var ELK = require('./elk.js');

module.exports = function(callback, jsonIn, thoroughness) {
    var data = JSON.parse(jsonIn);
    var elkJson = generateElkJson(data, thoroughness);
    var elk = new ELK();

    elk.layout(elkJson)
       .then(function (result) {
            var jsonOut = JSON.stringify(result);
            callback(null, jsonOut);
        }, function(err) {
            console.error('Tree layout failed!', err);
            callback(null, null);
       });
};

function generateElkJson(data, thoroughness) {
    var CARD_WIDTH = 300,
        CARD_HEIGHT = 100,
        SPACING = 30;

    // converts Bonsai-style JSON to ELK-style
    var nodes = [];
    var edges = [];

    processPersons();
    processRelations();

    return {
        id: 'root',
        layoutOptions: {
            'elk.algorithm': 'layered',
            'elk.direction': 'DOWN',
            'elk.edgeRouting': 'ORTHOGONAL',
            'elk.layered.spacing.edgeEdgeBetweenLayers': SPACING,
            'elk.layered.spacing.edgeNodeBetweenLayers': SPACING,
            'elk.spacing.nodeNode': SPACING,
            'elk.layered.nodePlacement.favorStraightEdges': false,
            'elk.layered.thoroughness': thoroughness
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