
Packery

    Layout
    Options
    Methods
    Draggable
    Events
    Extras

Draggable

    Draggabilly
    jQuery UI Draggable
    Dropping
    Drop placeholder
    Grid drop positions
    dragItemPositioned
    Item order

Packery layouts can be draggable, perfect for draggable Masonry layouts and draggable dashboards.

Draggable Masonry layout
Drag me!

Draggable dashboard
Drag me!
Draggabilly

Draggabilly is a separate library that makes elements draggable. It’s pretty great because it supports touch devices. After you initialize a new Draggabilly instance, bind its events to a Packery instance with bindDraggabillyEvents.

// jQuery
$grid.packery( 'bindDraggabillyEvents', draggie )
// vanilla JS
pckry.bindDraggabillyEvents( draggie )

draggie Draggabilly

// initialize Packery
var $grid = $('.grid').packery({
  itemSelector: '.grid-item',
  // columnWidth helps with drop positioning
  columnWidth: 100
});

// make all grid-items draggable
$grid.find('.grid-item').each( function( i, gridItem ) {
  var draggie = new Draggabilly( gridItem );
  // bind drag events to Packery
  $grid.packery( 'bindDraggabillyEvents', draggie );
});

/* highlight drag & drop */

/* Draggabilly adds is-dragging */
.grid-item.is-dragging,
/* Packery adds class while transitioning to drop position */
.grid-item.is-positioning-post-drag {
  background: #EA0;
  z-index: 2; /* keep dragged item on top */
}

Drag items

Edit this demo or vanilla JS demo on CodePen

Unbind Draggabilly events with unbindDraggabillyEvents.
jQuery UI Draggable

Packery works with jQuery UI Draggable. Bind Draggable events to Packery with bindUIDraggableEvents.

// jQuery
$grid.packery( 'bindUIDraggableEvents', $items )

$items jQuery Draggable item elements

// initialize Packery
var $grid = $('.grid').packery({
  itemSelector: '.grid-item',
  // columnWidth helps with drop positioning
  columnWidth: 100
});

// make all items draggable
var $items = $grid.find('.grid-item').draggable();
// bind drag events to Packery
$grid.packery( 'bindUIDraggableEvents', $items );

/* highlight drag & drop */

/* jQuery UI Draggable adds ui-draggable-dragging */
.grid-item.ui-draggable-dragging,
/* Packery adds class while transitioning to drop position */
.grid-item.is-positioning-post-drag {
  background: #EA0;
  z-index: 2; /* keep dragged item on top */
}

Drag items

Edit this demo on CodePen

Unbind jQuery UI Draggable events with unbindUIDraggableEvents.
Dropping
Drop placeholder

Packery will position a placeholder element, .packery-drop-placeholder, where a dragged element will be positioned when dropped. Style the drop placeholder with your own CSS as its unstyled by default.

.packery-drop-placeholder {
  outline: 3px dashed #444;
  outline-offset: -6px;
  /* transition position changing */
  -webkit-transition: -webkit-transform 0.2s;
          transition: transform 0.2s;
}

Drag items

Edit this demo or vanilla JS demo on CodePen
Grid drop positions

We recommend setting columnWidth (or rowHeight for horizontal layouts) to help align dropped items to a grid . Without columnWidth set, dragged items can only be dropped in place of other items.
dragItemPositioned

Use the dragItemPositioned event to detect when Packery positions a dropped item.
Item order

After dropping an item, you can get item elements in order with getItemElements.

// show item order after layout
function orderItems() {
  var itemElems = $grid.packery('getItemElements');
  $( itemElems ).each( function( i, itemElem ) {
    $( itemElem ).text( i + 1 );
  });
}

$grid.on( 'layoutComplete', orderItems );
$grid.on( 'dragItemPositioned', orderItems );

Drag items

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