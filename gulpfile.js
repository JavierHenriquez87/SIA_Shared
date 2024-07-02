const del = require('del');
const gulp = require('gulp');
const npmdist = require('gulp-npm-dist');
const rename = require('gulp-rename');
const sass = require('gulp-sass')(require('sass'));
const autoprefixer = require("gulp-autoprefixer");
const sourcemaps = require("gulp-sourcemaps");    
const cleanCSS = require('gulp-clean-css');

const paths = {
    base: {
        base: {
            dir: './'
        },
        node: {
            dir: './node_modules'
        }
    },
    src: {
        base: {
            dir: './wwwroot/',
            files: './wwwroot/**/*'
        },
        libs: {
            dir: './wwwroot/assets/libs'
        },
        css: {
            dir: './wwwroot/assets/css',
            files: './wwwroot/assets/css/**/*'
        },
        scss: {
            dir: './wwwroot/assets/scss',
            files: './wwwroot/assets/scss/**/*',
            main: './wwwroot/assets/scss/*.scss'
        }
    }
};


gulp.task('watch', function () {
  gulp.watch(paths.src.scss.files, gulp.series('scss'));
});


gulp.task('scss', function () {
  return gulp
      .src(paths.src.scss.main)
      .pipe(sourcemaps.init())
      .pipe(sass().on('error', sass.logError))
      .pipe(
          autoprefixer()
      )
      .pipe(gulp.dest(paths.src.css.dir))
      .pipe(cleanCSS())
      .pipe(
          rename({
              // rename app.css to icons.min.css
              suffix: ".min"
          })
      )
      .pipe(sourcemaps.write("./")) // source maps for icons.min.css
      .pipe(gulp.dest(paths.src.css.dir));
});

gulp.task('copy:libs', function () {
  return gulp
      .src(npmdist(), { base: paths.base.node.dir })
      .pipe(rename(function (path) {
          path.dirname = path.dirname.replace(/\/dist/, '').replace(/\\dist/, '');
      }))
      .pipe(gulp.dest(paths.src.libs.dir));
});

gulp.task('clean:SIA', function (callback) {
  del.sync(paths.src.libs.dir);
  callback();
});

gulp.task('build', gulp.series(gulp.parallel('clean:SIA', 'copy:libs'), 'scss'));
gulp.task('default', gulp.series(gulp.parallel('clean:SIA', 'copy:libs', 'scss'), gulp.parallel('watch')));
