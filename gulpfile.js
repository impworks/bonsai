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
    styles: {
        root: './Styles/style.scss',
        all: [
            './Styles/**/*.scss'
        ]
    },
    front: {
        scripts: {
            all: [
                './Areas/Front/Scripts/**/*.js'
            ]
        }
    },
    admin: {
        scripts: {
            all: [
                './Areas/Admin/Scripts/**/*.js'
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

gulp.task('styles', function() {
    gulp.src(config.styles.root)
        .pipe(sass())
        .pipe(concatcss('style.css'))
        .pipe(gulp.dest(config.assets.styles));
});

gulp.task('front.scripts', function () {
    gulp.src(config.front.scripts.all)
        .pipe(concatjs('front.js'))
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('admin.scripts', function () {
    gulp.src(config.admin.scripts.all)
        .pipe(concatjs('front.js'))
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('custom', ['styles', 'front.scripts', 'admin.scripts']);

gulp.task('custom.watch', function() {
    watch(
        config.styles.all,
        function () { gulp.start('styles'); }
    );

    watch(
        config.front.scripts.all,
        function () { gulp.start('front.scripts'); }
    );

    watch(
        config.admin.scripts.all,
        function () { gulp.start('admin.scripts'); }
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

gulp.task('build', ['vendor', 'custom']);