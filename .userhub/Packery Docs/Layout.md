
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Layout

    Item sizing
    Responsive layouts
    imagesLoaded

Item sizing

All sizing and styling of items is handled by your own CSS.

<div class="grid">
  <div class="grid-item"></div>
  <div class="grid-item grid-item--width2"></div>
  <div class="grid-item"></div>
  <div class="grid-item grid-item--height2"></div>
  ...
</div>

.grid-item {
  float: left;
  width: 60px;
  height: 60px;
  background: #e6e5e4;
  border: 2px solid #b6b5b4;
}

.grid-item--width2 { width: 120px; }
.grid-item--height2 { height: 120px; }

Edit this demo on CodePen
Responsive layouts

Item sizes can be set with percentages for responsive layouts. Set percentPosition: true so item positions are set with percentages to reduce adjustment transitions on window resize.

<div class="grid">
  <div class="grid-item"></div>
  <div class="grid-item grid-item--width2"></div>
  ...
</div>

/* fluid 5 columns */
.grid-item { width: 20%; }
/* 2 columns */
.grid-item--width2 { width: 40%; }

$('.grid').packery({
  percentPosition: true
})

Edit this demo on CodePen

A horizontal gap can appear if the Packery layout is triggering a scroll bar. Force the scrollbar to be visible in CSS to prevent this gap.

// force vertical scrollbar, prevent Packery gap
html {
  overflow-y: scroll;
}

imagesLoaded

Unloaded images can throw off Packery layouts and cause item elements to overlap. imagesLoaded resolves this issue.

Initialize Packery, then trigger layout after each image loads.

// init Packery
var $grid = $('.grid').packery({
  // options...
});
// layout Packery after each image loads
$grid.imagesLoaded().progress( function() {
  $grid.packery();
});

Edit this demo or vanilla JS demo on CodePen

Or, initialize Packery after all images have been loaded.

var $grid = $('.grid').imagesLoaded( function() {
  // init Packery after all images have loaded
  $grid.packery({
    // options...
  });
});

Edit this demo or vanilla JS demo on CodePen
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Metafizzy makes delightful web plugins & logos

Isotope

Filter & sort magical layouts
Infinite Scroll

Automatically add next page
Flickity

Touch, responsive, flickable carousels
Logo Pizza

Hot & ready logos for sale

    Fizzy School

    Lessons in JavaScript for jQuery newbies

Follow @metafizzyco on Twitter for Packery updates
