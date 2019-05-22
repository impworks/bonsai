let Canvas = require('canvas');
let assert = require('assert');
let fs = require('fs');
let pdf = require('pdfjs-dist');

module.exports = function(callback, path) {
    let rawBody = new Uint8Array(fs.readFileSync(path));
    let loadingTask = pdf.getDocument(rawBody);

    loadingTask.promise.then(function(pdfDocument) {
        // Get the first page.
        pdfDocument.getPage(1).then(function(page) {
            // Render the page on a Node canvas with 100% scale.
            let viewport = page.getViewport({ scale: 1.0 });
            let canvasFactory = new NodeCanvasFactory();
            let canvasAndContext = canvasFactory.create(viewport.width, viewport.height);
            let renderContext = {
                canvasContext: canvasAndContext.context,
                viewport: viewport,
                canvasFactory: canvasFactory
            };

            let renderTask = page.render(renderContext);
            renderTask.promise.then(function() {
                let image = canvasAndContext.canvas.toBuffer();
                callback(null, image.toString('base64'));
            });
        });
    }).catch(function(reason) {
        console.error(reason);
        callback(null, null);
    });
};

function NodeCanvasFactory() { }

NodeCanvasFactory.prototype = {
    create: function NodeCanvasFactory_create(width, height) {
        assert(width > 0 && height > 0, 'Invalid canvas size');
        let canvas = Canvas.createCanvas(width, height);
        let context = canvas.getContext('2d');
        return {
            canvas: canvas,
            context: context
        };
    },

    reset: function NodeCanvasFactory_reset(canvasAndContext, width, height) {
        assert(canvasAndContext.canvas, 'Canvas is not specified');
        assert(width > 0 && height > 0, 'Invalid canvas size');
        canvasAndContext.canvas.width = width;
        canvasAndContext.canvas.height = height;
    },

    destroy: function NodeCanvasFactory_destroy(canvasAndContext) {
        assert(canvasAndContext.canvas, 'Canvas is not specified');

        // Zeroing the width and height cause Firefox to release graphics
        // resources immediately, which can greatly reduce memory consumption.
        canvasAndContext.canvas.width = 0;
        canvasAndContext.canvas.height = 0;
        canvasAndContext.canvas = null;
        canvasAndContext.context = null;
    }
};