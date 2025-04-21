/* jQuery UI Datepicker */

$(function() {
    "use strict";
    $("#fromDate").datepicker({
        minDate: '-3M',
        maxDate: new Date(),
        changeMonth: false,
        numberOfMonths: 1,
        onClose: function(selectedDate) {
            $("#toDate").datepicker("option", "minDate", selectedDate);
        }
    });
    $("#toDate").datepicker({
        maxDate: new Date(),
        changeMonth: false,
        numberOfMonths: 1,
        onClose: function(selectedDate) {
            $("#fromDate").datepicker("option", "maxDate", selectedDate);
        }
    });

    $("#datepicker_multiple_months").datepicker({
        numberOfMonths: 3,
        showButtonPanel: true
    });

    $("#validity_datepicker").datepicker({
        numberOfMonths: 3,
        hideIfNoPrevNext: false,
        showButtonPanel: true
    });
    $(".datepicker").datepicker();
});
