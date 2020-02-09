$(function () {
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
		},
		click: function (event, data) {
			var node = data.node,
				orgEvent = data.originalEvent;

			if (node.folder) {
				if (node.expanded && node.data.href) {
					window.open(node.data.href, (orgEvent.ctrlKey || orgEvent.metaKey) ? "_blank" : node.data.target);
				}
			}
			else {
				if (node.data.href) {
					window.open(node.data.href, (orgEvent.ctrlKey || orgEvent.metaKey) ? "_blank" : node.data.target);
				}
			}
		}	});

	$("input[name=search]").on("keyup", function (e) {
		$.ui.fancytree.getTree().filterNodes($(this).val());
	});

	$("#tagcloud").jQCloud(words);
});