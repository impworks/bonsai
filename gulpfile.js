/// <binding ProjectOpened='custom.watch' />
var gulp = require('gulp'),
    gutil = require('gulp-util'),
    sass = require('gulp-sass'),
    concatcss = require('gulp-concat-css'),
    concatjs = require('gulp-concat'),
    mincss = require('gulp-clean-css'),
    minjs = require('gulp-uglify'),
    watch = require('gulp-watch');

// ================
// Configuration
// ================

var config = {
    vendor: {
        scripts: [
            './node_modules/jquery/dist/jquery.js',
            './node_modules/popper.js/dist/umd/popper.js',
            './node_modules/bootstrap/dist/js/bootstrap.js',
            './node_modules/magnific-popup/dist/jquery.magnific-popup.js',
            './node_modules/devbridge-autocomplete/dist/jquery.autocomplete.js',
            './node_modules/gijgo/js/gijgo.js',
            './node_modules/gijgo/js/messages/messages.ru-ru.js'
        ],
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


gulp.task('content', ['content.styles', 'content.scripts.front', 'content.scripts.admin', 'content.scripts.common']);

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
});

// ================
// Vendor tasks
// ================

gulp.task('vendor.scripts', function () {
    gulp.src(config.vendor.scripts)
        .pipe(concatjs('vendor.js'))
        .pipe(minjs({  }))
        .on('error', function (err) { gutil.log(gutil.colors.red('[Error]'), err.toString()); })
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('vendor.fonts', function() {
    gulp.src(config.vendor.fonts)
        .pipe(gulp.dest(config.assets.fonts));
});

gulp.task('vendor', ['vendor.scripts', 'vendor.fonts']);

gulp.task('build', ['vendor', 'content']);