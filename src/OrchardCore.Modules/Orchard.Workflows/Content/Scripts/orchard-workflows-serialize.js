
var saveLocal = function (localId) {
    var workflow = {
        Activities: [],
        Connections: []
    };

    var allActivities = $('.activity');
    for (var i = 0; i < allActivities.length; i++) {
        var activity = allActivities[i];

        workflow.Activities.push({
            Name: activity.viewModel.name,
            ClientId: activity.viewModel.clientId,
            Start: activity.viewModel.start,
            State: activity.viewModel.state,
            Left: $(activity).position().left,
            Top: $(activity).position().top
        });
    }

    var allConnections = jsPlumb.getConnections();
    for (var i = 0; i < allConnections.length; i++) {
        var connection = allConnections[i];

        workflow.Connections.push({
            SourceId: connection.sourceId,
            TargetId: connection.targetId,
            SourceEndpoint: connection.endpoints[0].outcome.Id,
            //targetEndpoint: connection.targetEndpoint
        });
    }
    // serialize the object
    sessionStorage.setItem(localId, JSON.stringify(workflow));
};

var updateActivities = function(localId) {
    var workflow = loadWorkflow(localId);
    if (workflow == null) {
        return;
    }

    // activities        
    if (updatedActivityState != null) {
        for (var i = 0; i < workflow.Activities.length; i++) {
            var activity = workflow.Activities[i];

            if (updatedActivityClientId == activity.ClientId) {
                // if an activity has been modified, update it
                activity.State = JSON.parse(updatedActivityState);
            }
        }

        displaySaveMessage();
    }
    
    sessionStorage.setItem(localId, JSON.stringify(workflow));
};

var loadActivities = function (localId) {
    var workflow = loadWorkflow(localId);
    if (workflow == null) {
        return;
    }
    
    // activities        
    for (var i = 0; i < workflow.Activities.length; i++) {
        var activity = workflow.Activities[i];
        renderActivity(activity.ClientId, activity.Id, activity.Name, activity.State, activity.Start, activity.Top, activity.Left);
    }

    // connections
    for (var i = 0; i < workflow.Connections.length; i++) {
        var connection = workflow.Connections[i];

        var source = document.getElementById(connection.SourceId);
        var ep = source.endpoints[connection.SourceEndpoint];

        jsPlumb.connect({
            source: ep,
            target: connection.TargetId,
            deleteEndpointsOnDetach: false
        });
    }

    refreshStateMessage();
    return workflow;
};

var loadWorkflow = function(localId) {
    var workflow = sessionStorage.getItem(localId);

    if (!workflow) {
        return;
    }

    // deserialize
    workflow = JSON.parse(workflow);
    

    return workflow;
};

var getActivity = function(localId, clientId) {

    var workflow = loadWorkflow(localId);
    if (workflow == null) {
        return;
    }

    var activity = null;
    for (var i = 0; i < workflow.Activities.length; i++) {
        var a = workflow.Activities[i];
        if (a.ClientId == clientId) {
            activity = a;
        }
    }

    return activity;
};

var loadForm = function(localId, clientId) {

    // bind state to form

    var activity = getActivity(localId, clientId);
    bindForm($('form'), activity.State);
};

var bindForm = function(form, data) {

    $.each(data, function (name, val) {
        var $el = $('[name="' + name + '"]'),
            tagName = $el.get(0).nodeName.toLowerCase(),
            type = $el.attr('type') && $el.attr('type').toLowerCase(),
            values = val.split(',');

        switch (tagName) {
            case 'input':
                switch (type) {
                    case 'checkbox':
                        $el.each(function () {
                            var self = $(this);
                            self.attr('checked', values.indexOf(self.attr('value')) != -1);
                        });
                        break;
                    case 'radio':
                        $el.filter('[value="' + val + '"]').attr('checked', 'checked');
                        break;
                    default:
                        $el.val(val);
                }
                break;
            case 'select':
                $el.val(values);
                //$el.find('option').each(function () {
                //    var self = $(this);
                //    self.attr('selected', values.indexOf(self.attr('value')) != -1);
                //});
                break;
            default:
                $el.val(val);
        }
    });
};
