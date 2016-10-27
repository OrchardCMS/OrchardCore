$(function() {

    var initializeFeaturesUI = function() {
        var bulkActions = $(".bulk-actions-wrapper").addClass("visible");
        //var theSwitch = $(".switch-for-switchable");
        //theSwitch.prepend(bulkActions);
        $("#search-box").keyup(function() {
            var text = $(this).val();

            if (text == '') {
                $("li.category").show();
                $("li.feature:hidden").show();
                return;
            }

            $("li.feature").each(function() {
                var elt = $(this);
                var value = elt.find('.title label:first').text();
                if (value.toLowerCase().indexOf(text.toLowerCase()) >= 0)
                    elt.show();
                else
                    elt.hide();
            });

            $("li.category:hidden").show();
            var toHide = $("li.category:not(:has(li.feature:visible))").hide();
        });
    };

    var initializeSelectionBehavior = function() {
        $("li.feature h3").on("change", "input[type='checkbox']", function() {
            var checked = $(this).is(":checked");
            var wrapper = $(this).parents("li.feature:first");
            wrapper.toggleClass("selected", checked);
        });
    };

    var initializeActionLinks = function() {
        $("li.feature .tools").on("click", "label[data-feature-action]", function(e) {
            var actionLink = $(this);
            var featureId = actionLink.data("feature-id");
            var action = actionLink.data("feature-action");
            var force = actionLink.data("feature-force");
            var dependants = actionLink.data("feature-dependants");

            if (!dependants || /^\s*$/.test(dependants) || confirm($("<div/>").html(confirmDisableMessage + "\n\n" + dependants).text())) {

                $("[name='submit.BulkExecute']").val("yes");
                $("[name='featureIds']").val(featureId);
                $("[name='bulkAction']").val(action);
                $("[name='force']").val(force);

                $("[name='submit.BulkExecute']").click();
            }

            e.preventDefault();
        });
    };

    initializeFeaturesUI();
    initializeSelectionBehavior();
    initializeActionLinks();
});