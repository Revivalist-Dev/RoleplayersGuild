
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Methods

    Layout
    layout / .packery()
    shiftLayout
    layoutItems
    fit
    stamp
    unstamp
    Adding & removing items
    appended
    prepended
    addItems
    remove
    Utilities
    reloadItems
    destroy
    getItemElements
    jQuery.fn.data('packery')
    Packery.data

Methods are actions done by Packery instances.

With jQuery, methods follow the jQuery UI pattern .packery( 'methodName' /* arguments */ ).

$grid.packery()
  .append( elem )
  .packery( 'appended', elem )
  // layout
  .packery();

jQuery chaining is broken by methods that return values: (i.e. getItemElements).

Vanilla JavaScript methods look like packery.methodName( /* arguments */ ). Unlike jQuery methods, they cannot be chained together.

// vanilla JS
var pckry = new Packery( '.grid', {...});
gridElement.appendChild( elem );
pckry.appended( elem );
pckry.layout();

Layout
layout / .packery()

Lays out all item elements. layout is useful when an item has changed size, and all items need to be laid out again.

// jQuery
$grid.packery()
// vanilla JS
pckry.layout()

var $grid = $('.grid').packery({
  itemSelector: '.grid-item'
});

$grid.on( 'click', '.grid-item', function( event ) {
  // change size of item by toggling large class
  $(  event.currentTarget  ).toggleClass('grid-item--large');
  // trigger layout after item size changes
  $grid.packery('layout');
});

Click item to toggle size

Edit this demo or vanilla JS demo on CodePen
shiftLayout

Shifts all item positions, maintaining their horizontal position.

After an item has changed, shiftLayout will move items up and down to best fit the change. This maintains more order compared to using layout/.packery(), where item positions can be completely changed.

// jQuery
$grid.packery('shiftLayout')
// vanilla JS
pckry.shiftLayout()

var $grid = $('.grid').packery({
  itemSelector: '.grid-item'
});

$grid.on( 'click', '.grid-item', function( event ) {
  // change size of item by toggling large class
  $( event.currentTarget ).toggleClass('grid-item--large');
  // trigger shiftLayout after item size changes
  $grid.packery('shiftLayout');
});

Click item to toggle size

Edit this demo or vanilla JS demo on CodePen
layoutItems

Lays out specified items.

// jQuery
$grid.packery( 'layoutItems', items, isInstant )
// vanilla JS
pckry.layoutItems( items, isInstant )

items Array of Packery.Items

isInstant Boolean Disables transitions
fit

Fit an item element within the layout, and shiftLayouts other item elements around it. fit is useful when expanding an element, and keeping it in its same position.

// jQuery
$container.packery( 'fit', element, x, y )
// vanilla JS
pckry.fit( element, x, y )

element Element

x Number Horizontal position, optional

y Number Vertical position, optional

$grid.on( 'click', '.grid-item', function( event ) {
  var $item = $( event.currentTarget );
  // change size of item by toggling large class
  $item.toggleClass('grid-item--large');
  if ( $item.is('.grid-item--large') ) {
    // fit large item
    $grid.packery( 'fit', event.currentTarget );
  } else {
    // back to small, shiftLayout back
    $grid.packery('shiftLayout');
  }
});

Click item to toggle size

Edit this demo or vanilla JS demo on CodePen

You can fit an element in a specific position.

$grid.on( 'click', '.grid-item', function( event ) {
  $grid.packery( 'fit', event.currentTarget, 120, 60 );
});

Click item to fit at 120, 60

Edit this demo or vanilla JS demo on CodePen
stamp

Stamps elements in the layout. Packery will lay out item elements around stamped elements.

// jQuery
$grid.packery( 'stamp', elements )
// vanilla JS
pckry.stamp( elements )

elements Element, jQuery Object, NodeList, or Array of Elements

Set itemSelector so that stamps do not get used as layout items.

var $grid = $('.grid').packery({
  // specify itemSelector so stamps do get laid out
  itemSelector: '.grid-item'
});
var $stamp = $grid.find('.stamp');
var isStamped = false;

$('.toggle-stamp-button').on( 'click', function() {
  // stamp or unstamp element
  if ( isStamped ) {
    $grid.packery( 'unstamp', $stamp );
  } else {
    $grid.packery( 'stamp', $stamp );
  }
  // trigger layout
  $grid.packery('layout');
  // set flag
  isStamped = !isStamped;
});

Edit this demo or vanilla JS demo on CodePen
unstamp

Un-stamps elements in the layout, so that Packery will no longer layout item elements around them. See demo above.

// jQuery
$grid.packery( 'unstamp', elements )
// vanilla JS
pckry.unstamp( elements )

elements Element, jQuery Object, NodeList, or Array of Elements
Adding & removing items
appended

Adds and lays out newly appended item elements to the end of the layout.

// jQuery
$grid.packery( 'appended', elements )
// vanilla JS
pckry.appended( elements )

elements Element, jQuery Object, NodeList, or Array of Elements

$('.append-button').on( 'click', function() {
  // create new item elements
  var $items = $('<div class="grid-item">...</div>');
  // append items to grid
  $grid.append( $items )
    // add and lay out newly appended items
    .packery( 'appended', $items );
});

Edit this demo or vanilla JS demo on CodePen
prepended

Adds and lays out newly prepended item elements at the beginning of layout.

// jQuery
pckry.prepended( elements )
// vanilla JS
$grid.packery( 'prepended', elements )

elements Element, jQuery Object, NodeList, or Array of Elements

$('.prepend-button').on( 'click', function() {
  // create new item elements
  var $items = $('<div class="grid-item">...</div>');
  // prepend items to grid
  $grid.prepend( $items )
    // add and lay out newly prepended items
    .packery( 'prepended', $items );
});

Edit this demo or vanilla JS demo on CodePen
addItems

Adds item elements to the Packery instance. addItems does not lay out items like appended or prepended.

// jQuery
$grid.packery( 'addItems', elements )
// vanilla JS
pckry.addItems( elements )

elements Element, jQuery Object, NodeList, or Array of Elements
remove

Removes elements from the Packery instance and DOM.

// jQuery
$grid.packery( 'remove', elements )
// vanilla JS
pckry.remove( elements )

elements Element, jQuery Object, NodeList, or Array of Elements

$grid.on( 'click', '.grid-item', function( event ) {
  // remove clicked element
  $grid.packery( 'remove', event.currentTarget )
    // shiftLayout remaining item elements
    .packery('shiftLayout');
});

Click items to remove

Edit this demo or vanilla JS demo on CodePen
Utilities
reloadItems

Recollects all item elements.

For frameworks like React and Angular, reloadItems may be useful to apply changes to the DOM to Packery.

// jQuery
$grid.packery('reloadItems')
// vanilla JS
pckry.reloadItems()

destroy

Removes the Packery functionality completely. destroy will return the element back to its pre-initialized state.

// jQuery
$grid.packery('destroy')
// vanilla JS
pckry.destroy()

var packeryOptions = {
  itemSelector: '.grid-item',
  columnWidth: 80
};
// initialize Packery
var $grid = $('.grid').packery( packeryOptions );
var isActive = true;

$('.toggle-button').on( 'click', function() {
  if ( isActive ) {
    $grid.packery('destroy'); // destroy
  } else {
    $grid.packery( packeryOptions ); // re-initialize
  }
  // set flag
  isActive = !isActive;
});

Edit this demo or vanilla JS demo on CodePen
getItemElements

Returns an array of item elements.

// jQuery
var elems = $grid.packery('getItemElements')
// vanilla JS
var elems = pckry.getItemElements()

elems Array of Elements
jQuery.fn.data('packery')

Get the Packery instance from a jQuery object. Packery instances are useful to access Packery properties.

var pckry = $('.grid').data('packery')
// access Packery properties
console.log( pckry.filteredItems.length + ' filtered items'  )

Packery.data

Get the Packery instance via its element. Packery.data() is useful for getting the Packery instance in JavaScript, after it has been initalized in HTML.

var pckry = Packery.data( element )

element Element or Selector String

pckry Packery

<!-- init Packery in HTML -->
<div class="grid" data-packery='{...}'>...</div>

// jQuery
// pass in the element, $element[0], not the jQuery object
var pckry = Packery.data( $('.grid')[0] )

// vanilla JS
// using an element
var grid = document.querySelector('.grid')
var pckry = Packery.data( grid )
// using a selector string
var pckry = Packery.data('.grid')

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
