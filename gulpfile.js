/// <binding ProjectOpened='front.watch' />
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
        styles: [
            './node_modules/bootstrap/dist/css/bootstrap.css',
            './node_modules/magnific-popup/dist/magnific-popup.css',
            './node_modules/font-awesome/css/font-awesome.css'
        ],
        scripts: [
            './node_modules/jquery/dist/jquery.js',
            './node_modules/popper.js/dist/umd/popper.js',
            './node_modules/bootstrap/dist/js/bootstrap.js',
            './node_modules/magnific-popup/dist/jquery.magnific-popup.js',
            './node_modules/devbridge-autocomplete/dist/jquery.autocomplete.js'
        ],
        fonts: [
            './node_modules/font-awesome/fonts/*.*'
        ]
    },
    front: {
        styles: {
            root: './Areas/Front/Content/Styles/style.scss',
            all: [
                './Areas/Front/Content/Styles/**/*.scss'
            ]
        },
        scripts: {
            all: [
                './Areas/Front/Content/Scripts/**/*.js'
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
// Front tasks
// ================

gulp.task('front.styles', function() {
    gulp.src(config.front.styles.root)
        .pipe(sass())
        .pipe(concatcss('front.css'))
        .pipe(gulp.dest(config.assets.styles));
});

gulp.task('front.scripts', function () {
    gulp.src(config.front.scripts.all)
        .pipe(concatjs('front.js'))
        .pipe(gulp.dest(config.assets.scripts));
});

gulp.task('front', ['front.styles', 'front.scripts']);

gulp.task('front.watch', function() {
    watch(
        config.front.styles.all,
        function () { gulp.start('front.styles'); }
    );

    watch(
        config.front.scripts.all,
        function () { gulp.start('front.scripts'); }
    );
});

// ================
// Vendor tasks
// ================

gulp.task('vendor.styles', function () {
    gulp.src(config.vendor.styles)
        .pipe(concatcss('vendor.css', { rebaseUrls: false }))
        .pipe(mincss())
        .pipe(gulp.dest(config.assets.styles));
});

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

gulp.task('vendor', ['vendor.styles', 'vendor.scripts', 'vendor.fonts']);

gulp.task('build', ['vendor', 'front']);