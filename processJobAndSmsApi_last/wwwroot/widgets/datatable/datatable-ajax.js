$.fn.dataTable.ext.errMode = function ( settings, helpPage, message ) { 
  console.log(message);
};

$.fn.dataTableExt.oPagination.prev_next = {
    "fnInit": function ( oSettings, nPaging, fnCallbackDraw )
    {
        nPrevious = document.createElement( 'span' );
        nNext = document.createElement( 'span' );
         
        nPrevious.innerHTML = '&lt;';
        nNext.innerHTML = '&gt;';
         
        nPrevious.className = "btn btn-primary";
        nNext.className="btn btn-primary";
        nPrevious.style.marginRight = '5px';
         
        nPaging.appendChild( nPrevious );
        nPaging.appendChild( nNext );
         
        $(nPrevious).click( function() {
            oSettings.oApi._fnPageChange( oSettings, "previous" );
            fnCallbackDraw( oSettings );
        } );
         
        $(nNext).click( function() {
            oSettings.oApi._fnPageChange( oSettings, "next" );
            fnCallbackDraw( oSettings );
        } );
         
        $(nPrevious).bind( 'selectstart', function () { return false; } );
        $(nNext).bind( 'selectstart', function () { return false; } );
    },
     
    "fnUpdate": function ( oSettings, fnCallbackDraw )
    {
         
    }
};
