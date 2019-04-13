/// <binding ProjectOpened='content.watch, build' />
const gulp = require('gulp');
const sass = require('gulp-sass');
const concatcss = require('gulp-concat-css');
const concatjs = require('gulp-concat');
const rename = require('gulp-rename');

// ================
// Configuration
// ================

 const config = {
    vendor: {
        scripts: {
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
                './node_modules/simplemde/dist/simplemde.min.js'
            ],
            vue: [
                './node_modules/vue/dist/vue.js'
            ]
        },
        fonts: [
            './node_modules/font-awesome/fonts/*.*',
            './node_modules/gijgo/fonts/*.*'
        ]
    },
    content: {
        styles: {
            root: './Areas/Common/Styles/style.scss',
            all: [
                './Areas/Common/Styles/**/*.scss'
            ]
        },
        scripts: {
            front: [
                './Areas/Front/Scripts/**/*.js'
            ],
            admin: [
                './Areas/Admin/Scripts/**/*.js'
            ],
            common: [
                './Areas/Common/Scripts/**/*.js'
            ],
            tree: [
                './Areas/Admin/BackendScripts/tree-layout.js'
            ]
        }
    },
    assets: {
        styles: './wwwroot/assets/styles/',
        scripts: './wwwroot/assets/scripts/',
        fonts: './wwwroot/assets/fonts/'
    }
};

// ================
// Bonsai tasks
// ================

const content_styles = function() {
    return gulp.src(config.content.styles.root)
               .pipe(sass())
               .pipe(concatcss('style.css'))
               .pipe(gulp.dest(config.assets.styles));
};

const content_scripts_front = () => {
    return gulp.src(config.content.scripts.front)
               .pipe(concatjs('front.js'))
               .pipe(gulp.dest(config.assets.scripts));
};

const content_scripts_common = () => {
    return gulp.src(config.content.scripts.common)
               .pipe(concatjs('common.js'))
               .pipe(gulp.dest(config.assets.scripts));
};

const content_scripts_admin = () => {
    return gulp.src(config.content.scripts.admin)
               .pipe(concatjs('admin.js'))
               .pipe(gulp.dest(config.assets.scripts));
};

const content_scripts_tree = () => {
    const outputFolder = './External/tree';
    const elkFolder = './node_modules/elkjs/lib/';
    const elkFiles = [
        config.content.scripts.tree[0],
        elkFolder + 'elk-api.js',
        elkFolder + 'elk-worker.min.js'
    ];

    return promisify(
        gulp.src(elkFiles).pipe(gulp.dest(outputFolder)),
        gulp.src(elkFolder + 'main.js').pipe(rename('elk.js')).pipe(gulp.dest(outputFolder))
    );
};

const content = gulp.parallel(
    content_styles,
    content_scripts_front,
    content_scripts_admin,
    content_scripts_common,
    content_scripts_tree
);

const watch = () => {
    gulp.watch(config.content.styles.all, content_styles);
    gulp.watch(config.content.scripts.front, content_scripts_front);
    gulp.watch(config.content.scripts.common, content_scripts_common);
    gulp.watch(config.content.scripts.admin, content_scripts_admin);
    gulp.watch(config.content.scripts.tree, content_scripts_tree);
};

// ================
// Vendor tasks
// ================

const vendor_scripts_common = () => {
    return gulp.src(config.vendor.scripts.common)
               .pipe(concatjs('vendor-common.js'))
               .pipe(gulp.dest(config.assets.scripts));
};

const vendor_scripts_admin = () => {
    return gulp.src(config.vendor.scripts.admin)
               .pipe(concatjs('vendor-admin.js'))
               .pipe(gulp.dest(config.assets.scripts));
};

const vendor_scripts_vue = () => {
    return gulp.src(config.vendor.scripts.vue)
               .pipe(concatjs('vendor-vue.js'))
               .pipe(gulp.dest(config.assets.scripts));
};

const vendor_fonts = function() {
    return gulp.src(config.vendor.fonts)
               .pipe(gulp.dest(config.assets.fonts));
};

const vendor = gulp.parallel(vendor_scripts_common, vendor_scripts_admin, vendor_scripts_vue, vendor_fonts);

const build = gulp.parallel(content, vendor);

module.exports = {
    content_styles,
    content_scripts_front,
    content_scripts_admin,
    content_scripts_common,
    content_scripts_tree,
    watch,
    content,
    vendor_scripts_common,
    vendor_scripts_admin,
    vendor_scripts_vue,
    vendor_fonts,
    vendor,
    build
};

function promisify(...elems) {
    return Promise.all(
        elems.map(x => new Promise(
            (ok, err) => x.on('error', err)
                          .on('end', ok)
        ))
    );
}