/*!
	Autosize v1.17.1 - 2013-06-23
	Automatically adjust textarea height based on user input.
	(c) 2013 Jack Moore - http://www.jacklmoore.com/autosize
	license: http://www.opensource.org/licenses/mit-license.php
*/
(function(e){var t,o={className:"autosizejs",append:"",callback:!1,resizeDelay:10},i='<textarea tabindex="-1" style="position:absolute; top:-999px; left:0; right:auto; bottom:auto; border:0; -moz-box-sizing:content-box; -webkit-box-sizing:content-box; box-sizing:content-box; word-wrap:break-word; height:0 !important; min-height:0 !important; overflow:hidden; transition:none; -webkit-transition:none; -moz-transition:none;"/>',n=["fontFamily","fontSize","fontWeight","fontStyle","letterSpacing","textTransform","wordSpacing","textIndent"],s=e(i).data("autosize",!0)[0];s.style.lineHeight="99px","99px"===e(s).css("lineHeight")&&n.push("lineHeight"),s.style.lineHeight="",e.fn.autosize=function(i){return i=e.extend({},o,i||{}),s.parentNode!==document.body&&e(document.body).append(s),this.each(function(){function o(){var o,a={};if(t=u,s.className=i.className,l=parseInt(h.css("maxHeight"),10),e.each(n,function(e,t){a[t]=h.css(t)}),e(s).css(a),"oninput"in u){var r=u.style.width;u.style.width="0px",o=u.offsetWidth,u.style.width=r}}function a(){var n,a,r,c;t!==u&&o(),s.value=u.value+i.append,s.style.overflowY=u.style.overflowY,a=parseInt(u.style.height,10),"getComputedStyle"in window?(c=window.getComputedStyle(u),r=u.getBoundingClientRect().width,e.each(["paddingLeft","paddingRight","borderLeftWidth","borderRightWidth"],function(e,t){r-=parseInt(c[t],10)}),s.style.width=r+"px"):s.style.width=Math.max(h.width(),0)+"px",s.scrollTop=0,s.scrollTop=9e4,n=s.scrollTop,l&&n>l?(u.style.overflowY="scroll",n=l):(u.style.overflowY="hidden",d>n&&(n=d)),n+=p,a!==n&&(u.style.height=n+"px",w&&i.callback.call(u,u))}function r(){clearTimeout(c),c=setTimeout(function(){h.width()!==z&&a()},parseInt(i.resizeDelay,10))}var l,d,c,u=this,h=e(u),p=0,w=e.isFunction(i.callback),f={height:u.style.height,overflow:u.style.overflow,overflowY:u.style.overflowY,wordWrap:u.style.wordWrap,resize:u.style.resize},z=h.width();h.data("autosize")||(h.data("autosize",!0),("border-box"===h.css("box-sizing")||"border-box"===h.css("-moz-box-sizing")||"border-box"===h.css("-webkit-box-sizing"))&&(p=h.outerHeight()-h.height()),d=Math.max(parseInt(h.css("minHeight"),10)-p||0,h.height()),h.css({overflow:"hidden",overflowY:"hidden",wordWrap:"break-word",resize:"none"===h.css("resize")||"vertical"===h.css("resize")?"none":"horizontal"}),"onpropertychange"in u?"oninput"in u?h.on("input.autosize keyup.autosize",a):h.on("propertychange.autosize",function(){"value"===event.propertyName&&a()}):h.on("input.autosize",a),i.resizeDelay!==!1&&e(window).on("resize.autosize",r),h.on("autosize.resize",a),h.on("autosize.resizeIncludeStyle",function(){t=null,a()}),h.on("autosize.destroy",function(){t=null,clearTimeout(c),e(window).off("resize",r),h.off("autosize").off(".autosize").css(f).removeData("autosize")}),a())})}})(window.jQuery||window.Zepto);

/* Textarea Character Count */

$(function () {

    $('.textarea-counter').keyup(function () {
      var max = 125;
      var len = $(this).val().length;
      if (len >= max) {
        $('.character-remaining').text(' you have reached the limit');
      } else {
        var char = max - len;
        $('.character-remaining').text(char + ' characters left');
      }
    });
	
	$('.message-counter').keyup(function () {
      var max = 160;
      var len = $(this).val().length;
      if (len >= max) {
        $('.character-remaining').text(' you have reached the limit');
      } else {
        var char = max - len;
        $('.character-remaining').text(char + ' characters left');
      }
    });
	
	
	$('.number-counter').keyup(function () {
      var max = 5000;
      var len = $(this).val().length;
      if (len >= max) {
        $('.character-remaining').text(' you have reached the limit');
      } else {
        var char = max - len;
        $('.character-remaining').text(char + ' numbers left');
      }
    });

});