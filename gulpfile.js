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
        styles: [
            './Areas/Front/Content/Styles/style.scss'
        ],
        scripts: [
            './Areas/Front/Content/Scripts/front.js'
        ]
    },
    assets: {
        root: './wwwroot/assets/'
    }
};

// ================
// Front tasks
// ================

gulp.task('front.styles', function() {
    gulp.src(config.front.styles)
        .pipe(sass())
        .pipe(concatcss('front.css'))
        .pipe(gulp.dest(config.assets.root));
});

gulp.task('front.scripts', function () {
    gulp.src(config.front.scripts)
        .pipe(concatjs('front.js'))
        .pipe(gulp.dest(config.assets.root));
});

gulp.task('front', ['front.styles', 'front.scripts']);

gulp.task('front.watch', function() {
    watch(
        config.front.styles,
        function () { gulp.start('front.styles'); }
    );

    watch(
        config.front.scripts,
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