$(function() {
  $("div.doc").hide();
  $("h2.doc").css({ cursor: "pointer" }).click(function() {
    $(this).next("div.doc").slideToggle();
  });
});
