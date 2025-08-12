
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Options

    Recommended
    itemSelector
    Layout
    columnWidth
    rowHeight
    gutter
    Element sizing
    percentPosition
    stamp
    horizontal
    originLeft
    originTop
    Setup
    containerStyle
    transitionDuration
    stagger
    shiftResize
    resize
    initLayout

All options are optional, but itemSelector is recommended.

// jQuery
$('.grid').packery({
  itemSelector: '.grid-item',
  gutter: 10
});

// vanilla JS
var pckry = new Packery( '.grid', {
  itemSelector: '.grid-item',
  gutter: 10
});

<!-- in HTML -->
<div class="grid" data-packery='{ "itemSelector": ".grid-item", "gutter": 10 }'>

Recommended
itemSelector

Specifies which child elements will be used as item elements in the layout.

We recommend always setting itemSelector. itemSelector is useful to exclude sizing elements or other elements that are not part of the layout.

itemSelector: '.grid-item'

<div class="grid">
  <!-- do not use banner in Packery layout -->
  <div class="static-banner">Static banner</div>
  <div class="grid-item"></div>
  <div class="grid-item"></div>
  ...
</div>

Static banner

Edit this demo on CodePen
Layout
columnWidth

Aligns items to a horizontal grid. columnWidth is not required for grid layouts, but may help align items when dragging.

columnWidth: 60

Edit this demo on CodePen

Use element sizing for responsive layouts with percentage widths. Set columnWidth to an Element or Selector String to use the outer width of the element for the size of the column.

<div class="grid">
  <!-- .grid-sizer empty element, only used for element sizing -->
  <div class="grid-sizer"></div>
  <div class="grid-item"></div>
  <div class="grid-item grid-item--width2"></div>
  ...
</div>

/* fluid 5 columns */
.grid-sizer,
.grid-item { width: 20%; }
/* 2 columns wide */
.grid-item--width2 { width: 40%; }

// use outer width of grid-sizer for columnWidth
columnWidth: '.grid-sizer',
itemSelector: '.grid-item',
percentPosition: true

Edit this demo on CodePen
rowHeight

Aligns items to a vertical grid. rowHeight is not required for grid layouts, but may help align items when dragging items in a horizontal layout.

rowHeight: 60

Edit this demo on CodePen

Use element sizing for responsive layouts with percentage widths. Set rowHeight to an Element or Selector String to use the outer height of the element for the size of the row.

<div class="grid">
  <!-- .grid-sizer empty element, only used for element sizing -->
  <div class="grid-sizer"></div>
  <div class="grid-item"></div>
  ...
</div>

/* height of 1 row */
.grid-sizer,
.grid-item { height: 60px; }
/* 2 rows high */
.grid-item--height2 { height: 120px; }

// use outer width of grid-sizer for columnWidth
itemSelector: '.grid-item',
rowHeight: '.grid-sizer',
percentPosition: true

Edit this demo on CodePen
gutter

Adds horizontal and vertical space between item elements.

gutter: 10

Edit this demo on CodePen

Use element sizing for responsive layouts with percentage widths. Set gutter to an Element or Selector String to use the outer width of the element.

<div class="grid">
  <div class="gutter-sizer"></div>
  <div class="grid-item"></div>
  <div class="grid-item grid-item--width2"></div>
  ...
</div>

.grid-item { width: 22%; }

.gutter-sizer { width: 4%; }

.grid-item--width2 { width: 48%; }

columnWidth: '.grid-sizer',
gutter: '.gutter-sizer',
itemSelector: '.grid-item',
percentPosition: true

Edit this demo on CodePen
Element sizing

Sizing options columnWidth, rowHeight, and gutter can be set with an element. The size of the element is then used as the value of the option.

<div class="grid">
  <!-- .grid-sizer & .gutter-sizer empty elements
    only used for element sizing -->
  <div class="grid-sizer"></div>
  <div class="gutter-sizer"></div>
  <div class="grid-item"></div>
  <div class="grid-item grid-item--width2"></div>
  ...
</div>

/* fluid 4 columns, 4% gutter */
.grid-sizer,
.grid-item { width: 22%; }

.gutter-sizer { width: 4%; }

/* 2 columns wide */
.grid-item--width2 { width: 40%; }

// use outer width of grid-sizer for columnWidth
columnWidth: '.grid-sizer',
gutter: '.gutter-sizer',
// do not use .grid-sizer in layout
itemSelector: '.grid-item',
percentPosition: true

Edit this demo on CodePen

Options can be set to a Selector String or an Element.

// set to a selector string
// first matching element within container element will be used
columnWidth: '.grid-sizer'

// set to an element
columnWidth: $grid.find('.grid-sizer')[0]

Element sizing options allow you to control the sizing of the Packery layout within your CSS. This is useful for responsive layouts and media queries.

/* 3 columns by default */
.grid-sizer { width: 33.333%; }

@media screen and (min-width: 768px) {
  /* 5 columns for larger screens */
  .grid-sizer { width: 20%; }
}

percentPosition

Sets item positions in percent values, rather than pixel values. percentPosition: true works well with percent-width items, as items will not transition their position on resize.

// set positions with percent values
percentPosition: true

/* fluid 5 columns */
.grid-item { width: 20%; }

Edit this demo on CodePen
stamp

Specifies which elements are stamped within the layout. Packery will layout items around stamped elements.

The stamp option stamps elements only when the Packery instance is first initialized. You can stamp additional elements afterwards with the stamp method.

<div class="grid">
  <div class="stamp stamp1"></div>
  <div class="stamp stamp2"></div>
  <div class="grid-item"></div>
  <div class="grid-item"></div>
  ....
</div>

// specify itemSelector so stamps do get laid out
itemSelector: '.grid-item',
// stamp elements
stamp: '.stamp'

/* position stamp elements with CSS */
.stamp {
  position: absolute;
  background: orange;
  border: 4px dotted black;
}
.stamp1 {
  left: 30%;
  top: 10px;
  width: 20%;
  height: 100px;
}
.stamp2 {
  right: 10%;
  top: 20px;
  width: 70%;
  height: 30px;
}

Edit this demo on CodePen
horizontal

Lays out items horizontally instead of vertically.

horizontal was previously isHorizontal in Packery v1. isHorizontal will still work in Packery v2.

horizontal: true

/* containers need height set when horizontal */
.grid {
  height: 300px;
}

Edit this demo on CodePen
originLeft

Controls the horizontal flow of the layout. By default, item elements start positioning at the left, with originLeft: true. Set originLeft: false for right-to-left layouts.

originLeft was previously isOriginLeft in Packery v1. isOriginLeft will still work in Packery v2.

originLeft: false

Edit this demo on CodePen
originTop

Controls the vertical flow of the layout. By default, item elements start positioning at the top, with originTop: true. Set originTop: false for bottom-up layouts. It’s like Tetris!

originTop was previously isOriginTop in Packery v1. isOriginTop will still work in Packery v2.

originTop: false

Edit this demo on CodePen
Setup
containerStyle

CSS styles that are applied to the container element.

// default
// containerStyle: { position: 'relative' }

// disable any styles being set on container
// useful if using absolute position on container
containerStyle: null

transitionDuration

Duration of the transition when items change position or appearance, set in a CSS time format. Default: transitionDuration: '0.4s'

// fast transitions
transitionDuration: '0.2s'

// slow transitions
transitionDuration: '0.8s'

// no transitions
transitionDuration: 0

stagger

Staggers item transitions, so items transition incrementally after one another. Set as a CSS time format, '0.03s', or as a number in milliseconds, 30.

stagger: 30

Click item to toggle size

Edit this demo or vanilla JS demo on CodePen
shiftResize
resize

Adjusts sizes and positions when window is resized. Enabled by default resize: true.

resize was previously isResizeBound in Packery v1. isResizeBound will still work in Packery v2.

// disable window resize behavior
resize: false

/* grid has fixed width */
.grid {
  width: 320px;
}

Edit this demo on CodePen
initLayout

Enables layout on initialization. Enabled by default initLayout: true.

Set initLayout: false to disable layout on initialization, so you can use methods or add events before the initial layout.

initLayout was previously isInitLayout in Packery v1. isInitLayout will still work in Packery v2.

var $grid = $('.grid').packery({
  // disable initial layout
  initLayout: false,
  //...
});
// bind event
$grid.packery( 'on', 'layoutComplete', function() {
  console.log('layout is complete');
});
// trigger initial layout
$grid.packery();

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
