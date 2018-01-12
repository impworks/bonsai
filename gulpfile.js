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
            './node_modules/magnific-popup/dist/jquery.magnific-popup.js'
        ]
    },
    front: {
        styles: {
            root: './Areas/Front/Content/Styles/styles.scss',
            all: [
                './Areas/Front/Content/Styles/**/*.scss'
            ]
        },
        scripts: {
            root: './Areas/Front/Content/Scripts/front.js',
            all: [
                './Areas/Front/Content/Scripts/**/*.js'
            ]
        }
    },
    assets: {
        root: './wwwroot/assets/'
    }
};

// ================
// Front tasks
// ================

gulp.task('front.styles', function() {
    gulp.src(config.front.styles.root)
        .pipe(sass())
        .pipe(concatcss('front.css'))
        .pipe(gulp.dest(config.assets.root));
});

gulp.task('front.scripts', function () {
    gulp.src(config.front.scripts.root)
        .pipe(concatjs('front.js'))
        .pipe(gulp.dest(config.assets.root));
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
        .pipe(concatcss('vendor.css'))
        .pipe(mincss())
        .pipe(gulp.dest(config.assets.root));
});

gulp.task('vendor.scripts', function () {
    gulp.src(config.vendor.scripts)
        .pipe(concatjs('vendor.js'))
        .pipe(minjs({  }))
        .on('error', function (err) { gutil.log(gutil.colors.red('[Error]'), err.toString()); })
        .pipe(gulp.dest(config.assets.root));
});

gulp.task('vendor', ['vendor.styles', 'vendor.scripts']);

gulp.task('build', ['vendor', 'front']);