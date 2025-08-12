
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Events

    Event binding
    jQuery event binding
    Vanilla JS event binding
    Packery events
    layoutComplete
    dragItemPositioned
    fitComplete
    removeComplete

Event binding
jQuery event binding

Bind events with jQuery with standard jQuery event methods .on(), .off(), and .one().

// jQuery
var $grid = $('.grid').packery({...});

function onLayout() {
  console.log('layout done');
}
// bind event listener
$grid.on( 'layoutComplete', onLayout );
// un-bind event listener
$grid.off( 'layoutComplete', onLayout );
// bind event listener to be triggered just once. note ONE not ON
$grid.one( 'layoutComplete', function() {
  console.log('layout done, just this one time');
});

jQuery event listeners have an event argument, whereas vanilla JS listeners do not.

// jQuery, has event argument
$grid.on( 'layoutComplete', function( event, items ) {
  console.log( items.length );
});

// vanilla JS, no event argument
pckry.on( 'layoutComplete', function( items ) {
  console.log( items.length );
});

Vanilla JS event binding

Bind events with vanilla JS with .on(), .off(), and .once() methods.

// vanilla JS
var pckry = new Packery( '.grid', {...});

function onLayout() {
  console.log('layout done');
}
// bind event listener
pckry.on( 'layoutComplete', onLayout );
// un-bind event listener
pckry.off( 'layoutComplete', onLayout );
// bind event listener to be triggered just once
pckry.once( 'layoutComplete', function() {
  console.log('layout done, just this one time');
});

Packery events
layoutComplete

Triggered after a layout and all positioning transitions have completed.

// jQuery
$grid.on( 'layoutComplete', function( event, laidOutItems ) {...} )
// vanilla JS
msnry.on( 'layoutComplete', function( laidOutItems ) {...} )

laidOutItems Array of Masonry.Items Items that were laid out

$grid.on( 'layoutComplete',
  function( event, laidOutItems ) {
    console.log( 'Packery layout completed on ' +
      laidOutItems.length + ' items' );
  }
);

Click item to toggle size

Edit this demo or vanilla JS demo on CodePen
dragItemPositioned

Triggered after a dragging an item and positioning transition has ended.

// jQuery
$grid.on( 'dragItemPositioned', function( event, draggedItem ) {...} )
// vanilla JS
pckry.on( 'dragItemPositioned', function( draggedItem ) {...} )

draggedItem Packery.Item

$grid.on( 'dragItemPositioned',
  function( event, draggedItem ) {
    console.log( 'Packery drag item positioned',
      draggedItem.element );
  }
);

Drag items

Edit this demo or vanilla JS demo on CodePen
fitComplete

Triggered after an item element is fit.

// jQuery
$grid.on( 'fitComplete', function( event, item ) {...} )
// vanilla JS
pckry.on( 'fitComplete', function( item ) {...} )

item Packery.Item

$grid.on( 'fitComplete',
  function( event, item ) {
    console.log( 'Packery fit', item.element );
  }
);

Click item to fit at 120, 60

Edit this demo or vanilla JS demo on CodePen
removeComplete

Triggered after an item element has been removed.

// jQuery
$grid.on( 'removeComplete', function( event, removedItems ) {...} )
// vanilla JS
msnry.on( 'removeComplete', function( removedItems ) {...} )

removedItems Array of Packery.Items Items that were removed

$grid.on( 'removeComplete',
  function( event, removedItems ) {
    notify( 'Removed ' + removedItems.length + ' items' );
  }
);

Click items to remove

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