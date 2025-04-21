/* Daterangepicker bootstrap */

$(function() {
    "use strict";

    $('#daterangepicker-example').daterangepicker();

    $('#daterangepicker-time').daterangepicker({
        timePicker: true,
        timePickerIncrement: 30,
        format: 'MM/DD/YYYY h:mm A'
    });

    $('#daterangepicker-custom').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'MM/DD/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }

        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }

        },
        function(start, end) {
            console.log("Callback has been called!");
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-custom-2').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'MM/DD/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }

        },
        function(start, end) {
            $('#daterangepicker-custom-2 span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom-2 span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-custom-1').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'MM/DD/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }
        },
        function(start, end) {
            $('#daterangepicker-custom-1 span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom-1 span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-2').daterangepicker({
        
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-3').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-4').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-5').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-6').daterangepicker({
            startDate: moment().subtract('days', 29),
            endDate: moment(),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
                'Last 7 Days': [moment().subtract('days', 6), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')],
                'Last Month': [moment().subtract('month', 1).startOf('month'), moment().subtract('month', 1).endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit:{
              months: 1
            }
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-current-month').daterangepicker({
            startDate: moment().startOf('month'),
            endDate: moment(),
            minDate: moment().startOf('month'),
            maxDate: moment().endOf('month'),
            dateLimit: {days: 30},
            showDropdowns: false,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
                'Today': [moment(), moment()],
                'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
        		'Till Date': [moment().startOf('month'), moment()],
                'This Month': [moment().startOf('month'), moment().endOf('month')]
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
           dateLimit: {days: 9},
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));
    $('#daterangepicker-date-range-last-onemonth').daterangepicker({
            startDate: moment().subtract('month', 1).startOf('month'),
            endDate: moment().subtract('month', 1).endOf('month'),
            minDate: moment().subtract('month', 1).startOf('month'),
            maxDate: moment().subtract('month', 1).endOf('month'),
            showDropdowns: false,
            showWeekNumbers: false,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit: {days: 9},
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );

    $('#daterangepicker-custom span').html(moment().subtract('month', 1).format('MMMM D, YYYY') + ' - ' + moment().subtract('month', 1).endOf('month').format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-last-twomonth').daterangepicker({
            startDate: moment().subtract('month', 2).startOf('month'),
            endDate: moment().subtract('month', 2).endOf('month'),
            minDate: moment().subtract('month', 2).startOf('month'),
            maxDate: moment().subtract('month', 2).endOf('month'),
            dateLimit: {days: 9},
            showDropdowns: false,
            showWeekNumbers: false,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('month', 2).format('MMMM D, YYYY') + ' - ' + moment().subtract('month', 2).endOf('month').format('MMMM D, YYYY'));

    $('#daterangepicker-date-range-yesterday').daterangepicker({
			startDate: moment().subtract('days', 1),
            endDate: moment().subtract('days', 1),
            maxDate: moment().subtract('days', 1),
            showDropdowns: true,
            showWeekNumbers: true,
            timePicker: false,
            timePickerIncrement: 1,
            timePicker12Hour: true,
            ranges: {
              'Today'           : [moment(), moment()],
              'Yesterday'		: [moment().subtract('days', 1), moment().subtract('days', 1)],
              'Till Date'		: [moment().startOf('month'), moment()],
              'This Month'  	: [moment().startOf('month'), moment().endOf('month')],
              'Last Month'  	: [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')],
            },
            opens: 'left',
            buttonClasses: ['btn btn-default'],
            applyClass: 'small bg-green',
            cancelClass: 'small ui-state-default',
            format: 'DD/MM/YYYY',
            separator: ' to ',
            locale: {
                applyLabel: 'Apply',
                fromLabel: 'From',
                toLabel: 'To',
                customRangeLabel: 'Custom Range',
                daysOfWeek: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
                monthNames: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
                firstDay: 1
            },
            linkedCalendars: true,
            dateLimit: {days: 9},
        },
        function(start, end) {
            $('#daterangepicker-custom span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));
        }
    );
    $('#daterangepicker-custom span').html(moment().subtract('days', 29).format('MMMM D, YYYY') + ' - ' + moment().format('MMMM D, YYYY'));

    $('#daterangepicker-detail-report-range').daterangepicker({
      startDate: moment(),
      endDate: moment(),
      minDate: moment().subtract('month', 2).startOf('month'),
      maxDate: moment(),
      showDropdowns: false,
      showWeekNumbers: false,
      timePicker: false,
      opens: 'left',
      buttonClasses: ['btn btn-default'],
      applyClass: 'small bg-green',
      cancelClass: 'small ui-state-default',
      format: 'DD/MM/YYYY',
      separator: ' to ',
      ranges: {
        'Today': [moment(), moment()],
        'Yesterday': [moment().subtract('days', 1), moment().subtract('days', 1)],
        'Till Date': [moment().startOf('month'), moment()],
      },
      locale: {
        applyLabel: 'Apply',
        fromLabel: 'From',
        toLabel: 'To',
        customRangeLabel: 'Custom Range',
        firstDay: 1
      },
      linkedCalendars: true,
      dateLimit:{
        months: 1
      }
    });
});
