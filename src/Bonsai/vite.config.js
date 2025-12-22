import { defineConfig } from 'vite';
import { viteStaticCopy } from 'vite-plugin-static-copy';
import { globSync } from 'glob';
import { minify } from 'terser';
import * as sass from 'sass';
import CleanCSS from 'clean-css';
import fs from 'fs';
import path from 'path';

// Bundle configurations matching gulpfile.js
const bundles = {
    content: {
        front: ['./Areas/Front/Scripts/**/*.js'],
        common: ['./Areas/Common/Scripts/**/*.js'],
        admin: ['./Areas/Admin/Scripts/**/*.js']
    },
    vendor: {
        common: [
            './node_modules/jquery/dist/jquery.js',
            './node_modules/popper.js/dist/umd/popper.js',
            './node_modules/bootstrap/dist/js/bootstrap.js',
            './node_modules/magnific-popup/dist/jquery.magnific-popup.js',
            './node_modules/devbridge-autocomplete/dist/jquery.autocomplete.js',
            './node_modules/toastr/toastr.js',
            './node_modules/selectize/dist/js/standalone/selectize.js',
            './node_modules/jquery-fullscreen-plugin/jquery.fullscreen.js',
            './Areas/Common/Libs/jquery-ui.js',
            './Areas/Common/Libs/gijgo.core.js',
            './Areas/Common/Libs/gijgo.datepicker.js',
            './Areas/Common/Libs/throttle.js'
        ],
        admin: [
            './node_modules/blueimp-file-upload/js/vendor/jquery.ui.widget.js',
            './node_modules/blueimp-file-upload/js/jquery.iframe-transport.js',
            './node_modules/blueimp-file-upload/js/jquery.fileupload.js',
            './node_modules/easymde/dist/easymde.min.js',
            './node_modules/rangeslider.js/dist/rangeslider.js'
        ],
        vue: {
            dev: './node_modules/vue/dist/vue.js',
            prod: './node_modules/vue/dist/vue.min.js'
        }
    }
};

// Custom plugin for SCSS compilation (bypasses Vite's URL processing)
function scssPlugin(mode) {
    const isProd = mode === 'production';

    return {
        name: 'scss-compile',
        async generateBundle() {
            // Compile SCSS using sass directly (preserves URLs as-is)
            const result = sass.compile('./Areas/Common/Styles/style.scss', {
                loadPaths: ['node_modules'],
                silenceDeprecations: ['import', 'global-builtin', 'color-functions', 'legacy-js-api']
            });

            let css = result.css;

            // Minify in production
            if (isProd) {
                const minified = new CleanCSS().minify(css);
                css = minified.styles;
            }

            this.emitFile({
                type: 'asset',
                fileName: 'styles/style.css',
                source: css
            });
        }
    };
}

// Custom plugin for JS concatenation (replicates gulp-concat behavior)
function concatPlugin(mode) {
    const isProd = mode === 'production';

    async function processContent(content) {
        if (isProd) {
            const result = await minify(content, {
                compress: true,
                mangle: true
            });
            return result.code;
        }
        return content;
    }

    return {
        name: 'concat-js',
        async generateBundle() {
            // Content bundles (use glob patterns)
            for (const [name, patterns] of Object.entries(bundles.content)) {
                const files = patterns.flatMap(p => globSync(p).sort());
                const content = files.map(f => fs.readFileSync(f, 'utf-8')).join('\n');
                this.emitFile({
                    type: 'asset',
                    fileName: `scripts/${name}.js`,
                    source: await processContent(content)
                });
            }

            // Vendor bundles (use explicit file lists)
            for (const [name, files] of Object.entries(bundles.vendor)) {
                if (name === 'vue') continue; // Handle separately
                const content = files.map(f => fs.readFileSync(f, 'utf-8')).join('\n');
                this.emitFile({
                    type: 'asset',
                    fileName: `scripts/vendor-${name}.js`,
                    source: await processContent(content)
                });
            }

            // Vue bundle (dev vs prod - Vue already provides minified version)
            const vueFile = isProd ? bundles.vendor.vue.prod : bundles.vendor.vue.dev;
            const vueContent = fs.readFileSync(vueFile, 'utf-8');
            this.emitFile({
                type: 'asset',
                fileName: 'scripts/vendor-vue.js',
                source: vueContent
            });
        }
    };
}

// Plugin to clean up the empty entry file after build
function cleanupPlugin() {
    return {
        name: 'cleanup',
        writeBundle() {
            const entryFile = path.resolve('wwwroot/assets/style-entry.js');
            if (fs.existsSync(entryFile)) {
                fs.unlinkSync(entryFile);
            }
            // Remove duplicate assets folder if it exists
            const assetsFolder = path.resolve('wwwroot/assets/assets');
            if (fs.existsSync(assetsFolder)) {
                fs.rmSync(assetsFolder, { recursive: true });
            }
        }
    };
}

export default defineConfig(({ mode }) => ({
    build: {
        outDir: 'wwwroot/assets',
        emptyOutDir: false,
        // We handle everything via plugins, this is just a stub entry
        rollupOptions: {
            input: 'src/style-entry.js',
            output: {
                entryFileNames: '[name].js'
            }
        }
    },
    plugins: [
        scssPlugin(mode),
        concatPlugin(mode),
        cleanupPlugin(),
        viteStaticCopy({
            targets: [
                // Fonts
                { src: 'node_modules/font-awesome/fonts/*', dest: 'fonts' },
                { src: 'node_modules/gijgo/fonts/*', dest: 'fonts' },
                { src: 'Areas/Common/Fonts/*', dest: 'fonts' },
                // ELK tree layout files (go to project root External/, not wwwroot/External/)
                { src: 'Areas/Admin/BackendScripts/tree-layout.js', dest: '../../External/tree' },
                { src: 'node_modules/elkjs/lib/elk-api.js', dest: '../../External/tree' },
                { src: 'node_modules/elkjs/lib/elk-worker.min.js', dest: '../../External/tree' },
                {
                    src: 'node_modules/elkjs/lib/main.js',
                    dest: '../../External/tree',
                    rename: 'elk.js'
                }
            ]
        })
    ]
}));
