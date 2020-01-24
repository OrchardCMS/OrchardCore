$(function(){
    $("#tree").fancytree({
      extensions: ["glyph", "wide", "filter"],
      debugLevel: 4,
      checkbox: false,
      selectMode: 3,
      quicksearch: true,
      glyph: {
        preset: "material",
        map: {}
      },
      filter: {
        autoApply: true,
        autoExpand: true,
        counter: false,
        fuzzy: false,
        hideExpandedCounter: true,
        hideExpanders: false,
        highlight: false,
        leavesOnly: false,
        nodata: true,
        mode: "hide"
      }
    });
 
    $("input[name=search]").on("keyup", function(e){
      $.ui.fancytree.getTree().filterNodes($(this).val());
    })

    var words = [
      {text: "Lorem", weight: 13},
      {text: "Ipsum", weight: 10.5},
      {text: "Dolor", weight: 9.4},
      {text: "Sit", weight: 8},
      {text: "Amet", weight: 6.2},
      {text: "Consectetur", weight: 5},
      {text: "Adipiscing", weight: 5},
      {text: "Loremss", weight: 13},
      {text: "Ipsumss", weight: 10.5},
      {text: "Dolorss", weight: 9.4},
      {text: "Siss", weight: 8},
      {text: "Ametss", weight: 6.2},
      {text: "Consecteturss", weight: 5},
      {text: "Adipiscingss", weight: 5},
    ];

    $("#tagcloud").jQCloud(words);
  });
