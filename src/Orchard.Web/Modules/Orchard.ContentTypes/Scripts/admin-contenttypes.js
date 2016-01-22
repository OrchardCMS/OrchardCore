(function($) {
    $(function() {
        $("#search-box").on("keyup", function (e) {
            var text = $(this).val();
            
            if (e.keyCode == 13) {
                var visibleRows = $("[data-record-text]:visible");
                if (visibleRows.length > 0) {
                    var editLink = $(".related a:last", visibleRows[0]);
                    location.href = editLink.attr("href");
                } else {
                    var primaryButton = $("#layout-main .manage .primaryAction");
                    location.href = primaryButton.attr("href") + "?suggestion=" + text;
                }
                return;
            }
            
            if (text == "") {
                $("[data-record-text]").show();
            } else {
                var lowerCaseText = text.toLowerCase();
                $("[data-record-text]").each(function() {
                    var recordText = $(this).data("record-text").toLowerCase();
                    $(this).toggle(recordText.indexOf(lowerCaseText) >= 0);
                });
            }
        });
        
        $("#layout-main .manage .primaryAction").on("click", function(e) {
            var suggestion = $("#search-box").val();

            if (suggestion.length == 0) {
                return;
            }

            location.href = $(this).attr("href") + "?suggestion=" + suggestion;
            e.preventDefault();
        });
    });
})(jQuery);