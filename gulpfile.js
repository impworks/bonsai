/// <binding ProjectOpened='content.watch, build' />
var gulp = require('gulp'),
    gutil = require('gulp-util'),
    sass = require('gulp-sass'),
    concatcss = require('gulp-concat-css'),
    concatjs = require('gulp-concat'),
    mincss = require('gulp-clean-css'),
    minjs = require('gulp-uglify'),
    watch = require('gulp-watch'),
    rename = require('gulp-rename');

// ================
// Configuration
// ================

var config = {
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

gulp.task('content.styles', function() {
    gulp.src(config.content.styles.root)
        .pipe(sass())
        .pipe(concatcss('style.css'))
        .pipe(gulp.dest(config.assets.styles));
});

gulp.task('content.scripts.front', function () {
    gulp.src(config.content.scripts.front)
        .pipe(concatjs('front.js'))
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('content.scripts.common', function () {
    gulp.src(config.content.scripts.common)
        .pipe(concatjs('common.js'))
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('content.scripts.admin', function () {
    gulp.src(config.content.scripts.admin)
        .pipe(concatjs('admin.js'))
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('content.scripts.tree', function () {
    var outputFolder = './External/tree';
    var elkFolder = './node_modules/elkjs/lib/';
    var elkFiles = [
        config.content.scripts.tree,
        elkFolder + 'elk-api.js',
        elkFolder + 'elk-worker.min.js'
    ];
    gulp.src(elkFiles).pipe(gulp.dest(outputFolder));
    gulp.src(elkFolder + 'main.js').pipe(rename('elk.js')).pipe(gulp.dest(outputFolder));
});

gulp.task('content', ['content.styles', 'content.scripts.front', 'content.scripts.admin', 'content.scripts.common', 'content.scripts.tree']);

gulp.task('content.watch', function() {
    watch(
        config.content.styles.all,
        function () { gulp.start('content.styles'); }
    );

    watch(
        config.content.scripts.front,
        function () { gulp.start('content.scripts.front'); }
    );

    watch(
        config.content.scripts.common,
        function () { gulp.start('content.scripts.common'); }
    );

    watch(
        config.content.scripts.admin,
        function () { gulp.start('content.scripts.admin'); }
    );

    watch(
        config.content.scripts.tree,
        function () { gulp.start('content.scripts.tree'); }
    );
});

// ================
// Vendor tasks
// ================

gulp.task('vendor.scripts.common', function () {
    gulp.src(config.vendor.scripts.common)
        .pipe(concatjs('vendor-common.js'))
        //.pipe(minjs({  }))
        .on('error', function (err) { gutil.log(gutil.colors.red('[Error]'), err.toString()); })
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('vendor.scripts.admin', function () {
    gulp.src(config.vendor.scripts.admin)
        .pipe(concatjs('vendor-admin.js'))
        //.pipe(minjs({  }))
        .on('error', function (err) { gutil.log(gutil.colors.red('[Error]'), err.toString()); })
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('vendor.scripts.vue', function () {
    gulp.src(config.vendor.scripts.vue)
        .pipe(concatjs('vendor-vue.js'))
        //.pipe(minjs({  }))
        .on('error', function (err) { gutil.log(gutil.colors.red('[Error]'), err.toString()); })
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('vendor.fonts', function() {
    gulp.src(config.vendor.fonts)
        .pipe(gulp.dest(config.assets.fonts));
});

gulp.task('vendor', ['vendor.scripts.common', 'vendor.scripts.admin', 'vendor.scripts.vue', 'vendor.fonts']);

gulp.task('build', ['vendor', 'content']);