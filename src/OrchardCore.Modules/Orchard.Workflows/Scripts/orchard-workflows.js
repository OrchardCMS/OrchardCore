    var connectorPaintStyle = {
        lineWidth: 2,
        strokeStyle: "#999",
        joinstyle: "round",
        //outlineColor: "white",
        //outlineWidth: 7
    };

    var connectorHoverStyle = {
        lineWidth: 2,
        strokeStyle: "#225588"
    };

    var sourceEndpointOptions = {
        endpoint: ["Dot", { cssClass: 'sourceEndpoint', radius: 5 }],
        paintStyle: { fillStyle: '#225588' },
        isSource: true,
        isTarget: false,
        deleteEndpointsOnDetach: false,
        connector: ["Flowchart"], // gap needs to be the same as makeTarget.paintStyle.radius
        connectorStyle: connectorPaintStyle,
        hoverPaintStyle: connectorHoverStyle,
        connectorHoverStyle: connectorHoverStyle,
        overlays: [["Label", { location: [3, -1.5], cssClass: "sourceEndpointLabel" }]]
    };

    jsPlumb.bind("ready", function () {

        jsPlumb.importDefaults({
            Anchor : "Continuous",
            // default drag options
            DragOptions: { cursor: 'pointer', zIndex: 2000 },
            // default to blue at one end and green at the other
            EndpointStyles: [{ fillStyle: '#225588' }],
            // blue endpoints 7 px; Blank endpoints.
            Endpoints: [["Dot", { radius: 7 }], ["Blank"]],
            // the overlays to decorate each connection with.  note that the label overlay uses a function to generate the label text; in this
            // case it returns the 'labelText' member that we set on each connection in the 'init' method below.
            ConnectionOverlays: [
                ["Arrow", { width: 12, length: 12, location: -5 }],
                // ["Label", { location: 0.1, id: "label", cssClass: "aLabel" }]
            ],
            ConnectorZIndex: 5
        });

        // updates the state of any edited activity
        updateActivities(localId);

        // deserialize the previously locally saved workflow
        loadActivities(localId);
        
        // create activity toolbar for controlling their states
        createToolbar();

        // a new connection is created
        jsPlumb.bind("jsPlumbConnection", function (connectionInfo) {
            // ...update your data model here.  The contents of the 'connectionInfo' are described below.
        });

        // a connection is detached
        jsPlumb.bind("jsPlumbConnectionDetached", function (connectionInfo) {
            // ...update your data model here.  The contents of the 'connectionInfo' are described below.
        });

    });

    // instanciates a new workflow widget in the editor
    var createActivity = function (activityName, top, left) {
        return renderActivity(null, -1, activityName, {}, false, top, left);
    };

    
    // create a new activity node on the editor
    $('.activity-toolbox-item').draggable({ helper: 'clone' });
    $('#activity-editor').droppable({ drop: function(event, ui) {
        var activityName = ui.draggable.data('activity-name');
        if (activityName && activityName.length) {
            var offset = $(this).offset();
            if (displaySaveMessage()) {
                createActivity(activityName, event.pageY - offset.top - 40, event.pageX - offset.left); /* The displaySaveMessage's height is 40px */
            }
            else {
                createActivity(activityName, event.pageY - offset.top, event.pageX - offset.left);
            }
        }
        if (displaySaveMessage()) {
            var activityPosition = ui.position;
            activityPosition.top += 40; /* The displaySaveMessage's height is 40px */
        }
    }
    });

    $("#search-box").focus().on("keyup", function (e) {
        var text = $(this).val();
        if (text == "") {
            $(".activity-toolbox-item").show();
        } else {
            var lowerCaseText = text.toLowerCase();
            $(".activity-toolbox-item").each(function () {
                var recordText = $(this).data("activity-name").toLowerCase();
                $(this).toggle(recordText.indexOf(lowerCaseText) >= 0);
            });
        }
    });

    var renderActivity = function (clientId, id, name, state, start, top, left) {

        $.ajax({
            type: 'POST',
            url: renderActivityUrl,
            data: { name: name, state: state, __RequestVerificationToken: requestAntiForgeryToken },
            async: false,
            success: function(data) {
                var dom = $($.parseHTML($.trim(data)));

                if (dom == null) {
                    return null;
                }

                dom.addClass('activity');
                
                if ($.inArray(id, awaitingRecords) != -1) {
                    dom.addClass('awaiting');
                }
                    
                if (start) {
                    dom.addClass('start');
                }

                if (clientId) {
                    dom.attr('id', clientId);
                }

                var editor = $('#activity-editor');
                editor.append(dom);

                jsPlumb.draggable(dom, { containment: "parent", scroll: true, drag: hideToolbar });

                jsPlumb.makeTarget(dom, {
                    dropOptions: { hoverClass: "dragHover" },
                    anchor: "Continuous",
                    endpoint: "Blank",
                    paintStyle: { fillStyle: "#558822", radius: 3 },
                });

                var elt = dom.get(0);
                elt.viewModel = {
                    name: name,
                    state: state,
                    start: start,
                    clientId: dom.attr("id"),
                    hasForm: activities[name].hasForm
                };

                elt.endpoints = {};

                var outcomes = activities[name].outcomes;
                
                if (dom.data('outcomes')) {
                    outcomes = eval('[' + dom.data('outcomes') + ']');
                }
                
                for (i = 0; i < outcomes.length; i++) {
                    var ep = jsPlumb.addEndpoint(dom, {
                        anchor: "Continuous",
                        connectorOverlays: [["Label", { label: outcomes[i].Label, cssClass: "connection-label" }]],
                    },
                        sourceEndpointOptions);

                    elt.endpoints[outcomes[i].Id] = ep;
                    ep.outcome = outcomes[i];
                    // ep.overlays[0].setLabel(outcomes[i].Label);
                }

                if (activities[name].hasForm) {
                    var edit = function() {
                        saveLocal(localId);
                        window.location.href = editActivityUrl + "/" + $("#id").val() + "?name=" + name + "&clientId=" + elt.viewModel.clientId + "&localId=" + localId;
                    };
                    
                    dom.dblclick(edit);
                    elt.viewModel.edit = edit;
                }

                var canvasWidth = $('#activity-editor').width();
                var domWidth = $('#' + clientId).width() + 25; /* width + padding */

                dom.css('top', top + 'px');
                dom.css('left', left + domWidth > canvasWidth ? canvasWidth - domWidth : left + 'px');
                jsPlumb.repaint(elt.viewModel.clientId);
                
                dom.on("click", function () {
                    var self = $(this);
                    var toolbar = $('#activity-toolbar');


                    refreshToolbar(this);

                    toolbar.position({
                        my: "right bottom",
                        at: "right top",
                        offset: "0 -5",
                        of: self,
                        collision: "none"
                    });

                    toolbar.get(0).target = this;
                    toolbar.show();

                    return false;
                });
            }
            
        });

    };

    var createToolbar = function () {
        var editor = $('#activity-editor');

        // editor.focus(function () {
        editor.on("click", function () {
            hideToolbar();
        });

        initToolbar();
    };
    
    var initToolbar = function() {
        $('#activity-toolbar-start-checkbox').change(function () {
            var toolbar = $('#activity-toolbar');
            var target = $(toolbar).get(0).target;
            //var clientId = target.attr('id');
            //var activity = getActivity(localId, clientId);
            var checked = $(this).is(':checked');
            target.viewModel.start = checked;
            $(target).toggleClass('start', checked);
            
            // display a warning if there are no activities with a start state
            refreshStateMessage();

            displaySaveMessage();
        });

        // prevent the editor from getting clicked when the label is clicked
        $('#activity-toolbar-start').click(function (event) {
            event.stopPropagation();
        });
    };

    function refreshStateMessage() {
        if ($("#activity-editor div").hasClass('start')) {
            $("#start-message").hide();
        } else {
            $("#start-message").show();
        }
    }
    
    function displaySaveMessage() {
        var saveMessage = $("#save-message");

        if (saveMessage.css('display') === "none") {
            saveMessage.show();
            return true;
        }
        else {
            return false;
        }
    }

    var refreshToolbar = function(target) {
        target = $(target);

        // start button
        $('#activity-toolbar-start').toggle(target.hasClass('canStart'));
        $('#activity-toolbar-start-checkbox').prop('checked', target.get(0).viewModel.start);

        // edit button
        var editButton = $('#activity-toolbar-edit');
        if (target.get(0).viewModel.hasForm) {
            editButton.unbind("click").click(target.get(0).viewModel.edit);
            editButton.toggle(true);
        } else {
            editButton.toggle(false);
        }

        // delete button
        var deleteButton = $('#activity-toolbar-delete');
        deleteButton.unbind("click").click(function () {
            if (!confirm($("#confirm-delete-activity").val())) {
                return false;
            }
            
            jsPlumb.removeAllEndpoints(target.attr('id'));
            target.remove();

            displaySaveMessage();
        });

    };
    
    // hides the 
    var hideToolbar = function () {
        var toolbar = $('#activity-toolbar');
        toolbar.offset({ top: 0, left: 0 });
        toolbar.hide();
    };
