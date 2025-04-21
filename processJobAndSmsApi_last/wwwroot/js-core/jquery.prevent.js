document.oncontextmenu = function () {
  return false;
};
document.onselectstart = function () {
  if (event.srcElement.type != "text" && event.srcElement.type != "textarea" && event.srcElement.type != "password") {
    return false;
  } else {
    return true;
  }
};
if (window.sidebar) {
  document.onmousedown = function (colene) {
    var aalon = colene.target;
    if (aalon.tagName.toUpperCase() == "SELECT" || aalon.tagName.toUpperCase() == "INPUT" || aalon.tagName.toUpperCase() == "TEXTAREA" || aalon.tagName.toUpperCase() == "PASSWORD") {
      return true;
    } else {
      return false;
    }
  };
}
;
document.ondragstart = function () {
  return false;
};
document.onkeydown = function (deiveon) {
  if (deiveon.ctrlKey && (deiveon.keyCode === 67 || deiveon.keyCode === 86 || deiveon.keyCode === 85 || deiveon.keyCode === 117)) {
    return false;
  } else {
    return true;
  }
};
(function () {
  var jakelle = console;
  Object.defineProperty(window, "console", {get: function () {
    if (jakelle._commandLineAPI) {
      throw "Sorry, Can't execute scripts!";
    }
    ;
    return jakelle;
  }, set: function (dashiya) {
    jakelle = dashiya;
  }});
}());

