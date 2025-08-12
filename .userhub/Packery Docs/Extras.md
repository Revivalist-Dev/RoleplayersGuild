
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Extras

    Extra demos
    Module loaders
    RequireJS
    Browserify
    Webpack
    Bootstrap
    Animating item size
    Web fonts
    Typekit
    Issues
    Reduced test cases
    Submitting issues
    Browser support
    Upgrading from v1
    FAQ
    How do I fix overlapping item elements?
    What is the difference between Isotope, Masonry, and Packery?

Extra demos

    Centered container See Issue #8
    Appending items, making them draggable (for infinite scroll). See Issue #50
    Show empty spaces via canvas
    Ignore item element
    Fit item in previous position
    Disable & enable drag, with Draggabilly or with vanilla JS

Module loaders
RequireJS

Packery supports RequireJS.

You can require packery.pkgd.js.

requirejs( [
  'path/to/packery.pkgd.js',
], function( Packery ) {
  new Packery( '.grid', {...});
});

To use Packery as a jQuery plugin with RequireJS and packery.pkgd.js, you need to use jQuery Bridget.

// require the require function
requirejs( [ 'require', 'jquery', 'path/to/packery.pkgd.js' ],
  function( require, $, Packery ) {
    // require jquery-bridget, it's included in packery.pkgd.js
    require( [ 'jquery-bridget/jquery-bridget' ],
    function( jQueryBridget ) {
      // make Packery a jQuery plugin
      jQueryBridget( 'packery', Packery, $ );
      // now you can use $().packery()
      $('.grid').packery({...});
    }
  );
});

Or, you can manage dependencies with Bower. Set baseUrl to bower_components and set a path config for all your application code.

requirejs.config({
  baseUrl: 'bower_components/',
  paths: {
    app: '../'
  }
});

requirejs( [
  'packery/js/packery',
  'app/my-component.js'
], function( Packery, myComp ) {
  new Packery( '.grid', {...});
});

You can require Bower dependencies and use Packery as a jQuery plugin with jQuery Bridget.

requirejs.config({
  baseUrl: '../bower_components',
  paths: {
    jquery: 'jquery/dist/jquery'
  }
});

requirejs( [
    'jquery',
    'packery/js/packery',
    'jquery-bridget/jquery-bridget'
  ],
  function( $, Packery, jQueryBridget ) {
    // make Packery a jQuery plugin
    jQueryBridget( 'packery', Packery, $ );
    // now you can use $().packery()
    $('.grid').packery({...});
});

Browserify

Packery works with Browserify. Install Packery with npm.

npm install packery

var Packery = require('packery');

var pckry = new Packery( '.grid', {
  // options...
});

To use Packery as a jQuery plugin with Browserify, you need to use jQuery Bridget

npm install jquery
npm install jquery-bridget

var $ = require('jquery');
var jQueryBridget = require('jquery-bridget');
var Packery = require('packery');
// make Packery a jQuery plugin
jQueryBridget( 'packery', Packery, $ );
// now you can use $().packery()
$('.grid').packery({
  columnWidth: 80
});

Webpack

Install Packery with npm.

npm install packery

You can then require('packery').

// main.js
var Packery = require('packery');

var pckry = new Packery( '.grid', {
  // options...
});

Run webpack.

webpack main.js bundle.js

jQuery plugin functionality needs to be installed separately, similar to using Browserify.

npm install jquery-bridget

var $ = require('jquery');
var jQBridget = require('jquery-bridget');
var Packery = require('packery');
// make Packery a jQuery plugin
$.bridget( 'packery', Packery, $ );
// now you can use $().packery()
$('.grid').packery({
  columnWidth: 80
});

Bootstrap

You can use Packery layouts with Bootstrap v3 grid system. This example will display a fluid grid of 3 columns, using col-xs-4 as columnWidth. columnWidth is not required for this layout, but may be helpful if the layout is draggable.

<div class="container-fluid">
  <!-- add extra container element for Packery -->
  <div class="grid">
    <!-- add sizing element for columnWidth -->
    <div class="grid-sizer col-xs-4"></div>
    <!-- items use Bootstrap .col- classes -->
    <div class="grid-item col-xs-8">
      <!-- wrap item content in its own element -->
      <div class="grid-item-content">...</div>
    </div>
    <div class="grid-item col-xs-4">
      <div class="grid-item-content">...</div>
    </div>
    ...
  </div>
</div>

$('.grid').packery({
  // use a separate class for itemSelector, other than .col-
  itemSelector: '.grid-item', 
  columnWidth: '.grid-sizer',
  percentPosition: true
});

Edit this demo on CodePen

Use multiple .col- classes on item elements to use Bootstrap’s grid media queries to responsively change column sizes. This example will use 2, then 3, then 4 columns at different screen sizes.

<div class="container-fluid">
  <div class="grid">
    <!-- 2 col grid @ xs, 3 col grid @ sm, 4 col grid @ md -->
    <div class="grid-sizer col-xs-6 col-sm-4 col-md-3"></div>
    <!-- 1 col @ xs, 2 col @ sm, 2 col @ md -->
    <div class="grid-item col-xs-6 col-sm-8 col-md-6">
      <div class="grid-item-content">...</div>
    </div>
    <!-- 1 col @ xs, 1 col @ sm, 1 col @ md -->
    <div class="grid-item col-xs-6 col-sm-4 col-md-3">
      <div class="grid-item-content">...</div>
    </div>
    ...
  </div>
</div>

Edit this demo on CodePen
Animating item size

You cannot transition or animate the size of an item element and properly lay out. But there is a trick — you can animate a child element of the item element.

<div class="grid">
  <!-- items have grid-item-content child elements -->
  <div class="grid-item">
    <div class="grid-item-content"></div>
  </div>
  ...

/* item is invisible, but used for layout
item-content is visible, and transitions size */
.grid-item,
.grid-item-content {
  width: 60px;
  height: 60px;
}
.grid-item-content {
  background: #C09;
  transition: width 0.4s, height 0.4s;
}

/* both item and item content change size */
.grid-item.is-expanded,
.grid-item.is-expanded .grid-item-content {
  width: 180px;
  height: 120px;
}

Click to item to toggle size

Edit this demo or vanilla JS demo on CodePen

This technique works on items with responsive, percentage widths. Although, it does require a bit more JS. Check out the example on CodePen to see how it’s done.

.grid-item {
  width: 20%;
  height: 60px;
}

.grid-item-content {
  width: 100%;
  height: 100%;
  background: #C09;
  transition: width 0.4s, height 0.4s;
}
/* item has expanded size */
.grid-item.is-expanded {
  width: 60%;
  height: 120px;
}

Click to item to toggle size

Edit this demo or vanilla JS demo on CodePen
Web fonts

Like images, unloaded web fonts can throw off Packery. To resolve this, trigger layout after fonts have been loaded. Both Typekit and Google WebFont Loader provide font events to control scripts based on how fonts are loaded.

    Typekit font events
    Google WebFont Loader: Events

Typekit

Try the script below when using Packery on a page with Typekit. This will trigger Packery when the document is ready and again when fonts have loaded. Be sure to remove Typekit’s default script, try{Typekit.load();}catch(e){}.

var pckry;

function triggerPackery() {
  // don't proceed if packery has not been initialized
  if ( !pckry ) {
    return;
  }
  pckry.layout();
}
// initialize packery on document ready
docReady( function() {
  var container = document.querySelector('.grid');
  pckry = new Packery( container, {
    // options...
  });
});
// trigger packery when fonts have loaded
Typekit.load({
  active: triggerPackery,
  inactive: triggerPackery
});

// or with jQuery
var $grid;

function triggerPackery() {
  // don't proceed if $grid has not been selected
  if ( !$grid ) {
    return;
  }
  // init Packery
  $grid.packery({
    // options...
  });
}
// trigger packery on document ready
$(function(){
  $grid = $('.grid');
  triggerPackery();
});
// trigger packery when fonts have loaded
Typekit.load({
  active: triggerPackery,
  inactive: triggerPackery
});

Issues
Reduced test cases

Creating a reduced test case is the best way to debug problems and report issues. Read CSS Tricks on why they’re so great.

Create a reduced test case for Packery by forking any one of the CodePen demos from these docs.

    A reduced test case clearly demonstrates the bug or issue.
    It contains the bare minimum HTML, CSS, and JavaScript required to demonstrate the bug.
    A link to your production site is not a reduced test case.

Creating a reduced test case is the best way to get your issue addressed. They help you point out the problem. They help us debug the problem. They help others understand the problem.
Submitting issues

Report issues on GitHub. Make sure to include a reduced test case. Without a reduced test case, your issue may be closed.
Browser support

Packery v2 supports IE10+, Android 4+, Safari for iOS 5+, Firefox 16+, and Chrome 12+.

For IE8+ and Android 2.3 support, try Packery v1.
Upgrading from v1

Packery v2 dropped browser support for IE8, IE9, and Android 2.3. All options, methods, and code for Packery v1 is backwards compatibile with Packery v2.

    isOptionName options renamed to optionName. Packery v2 is backwards compatible with the old isOptionName options.
        isHorizontal → horizontal
        isOriginLeft → originLeft
        isOriginTop → originTop
        isResizeBound → resize
        isInitLayout → initLayout
    HTML initialization can be done with data-packery. Packery v2 is backwards compatible with previous code: js-packery class and data-packery-options attribute.
    IE8 helper dependencies removed: eventie, get-style-property, doc-ready
    jquery-bridget/jquery.bridget path renamed to jquery-bridget/jquery-bridget

FAQ
How do I fix overlapping item elements?

You need to initialize or trigger `layout/.packery()` after all the items have their proper size. If your elements have images, use imagesLoaded. See also Web fonts.
What is the difference between Isotope, Masonry, and Packery?

Isotope, Masonry, and Packery are all similar in that they are layout libraries. Many of their options and methods are the same.

Masonry and Packery have different layout algorithms. Both can achieve “masonry”, but Packery uses a bin-packing algorithm which fills in gaps and allows for layouts to be draggable.

Isotope does sorting and filtering. Isotope uses masonry and packery layouts, as well as other layouts.

Masonry is licensed MIT and is freely available for use and distribution. Isotope and Packery have a proprietary license, where you can purchase a license for commercial projects, or use it freely for open-source projects.
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