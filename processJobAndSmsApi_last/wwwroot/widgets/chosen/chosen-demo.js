
/* Chosen select */

$(function() { "use strict";
try{
    $(".chosen-select").chosen();
}catch(e){
  console.warn(e);
}
    $(".chosen-search").append('<i class="glyph-icon icon-search"></i>');
    $(".chosen-single div").html('<i class="glyph-icon icon-caret-down"></i>');

});
