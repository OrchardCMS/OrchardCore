// modules are defined as an array
// [ module function, map of requires ]
//
// map of requires is short require name -> numeric require
//
// anything defined in a previous bundle is accessed via the
// orig method which is the require for previous bundles

(function (modules, entry, mainEntry, parcelRequireName, globalName) {
  /* eslint-disable no-undef */
  var globalObject =
    typeof globalThis !== 'undefined'
      ? globalThis
      : typeof self !== 'undefined'
      ? self
      : typeof window !== 'undefined'
      ? window
      : typeof global !== 'undefined'
      ? global
      : {};
  /* eslint-enable no-undef */

  // Save the require from previous bundle to this closure if any
  var previousRequire =
    typeof globalObject[parcelRequireName] === 'function' &&
    globalObject[parcelRequireName];

  var cache = previousRequire.cache || {};
  // Do not use `require` to prevent Webpack from trying to bundle this call
  var nodeRequire =
    typeof module !== 'undefined' &&
    typeof module.require === 'function' &&
    module.require.bind(module);

  function newRequire(name, jumped) {
    if (!cache[name]) {
      if (!modules[name]) {
        // if we cannot find the module within our internal map or
        // cache jump to the current global require ie. the last bundle
        // that was added to the page.
        var currentRequire =
          typeof globalObject[parcelRequireName] === 'function' &&
          globalObject[parcelRequireName];
        if (!jumped && currentRequire) {
          return currentRequire(name, true);
        }

        // If there are other bundles on this page the require from the
        // previous one is saved to 'previousRequire'. Repeat this as
        // many times as there are bundles until the module is found or
        // we exhaust the require chain.
        if (previousRequire) {
          return previousRequire(name, true);
        }

        // Try the node require function if it exists.
        if (nodeRequire && typeof name === 'string') {
          return nodeRequire(name);
        }

        var err = new Error("Cannot find module '" + name + "'");
        err.code = 'MODULE_NOT_FOUND';
        throw err;
      }

      localRequire.resolve = resolve;
      localRequire.cache = {};

      var module = (cache[name] = new newRequire.Module(name));

      modules[name][0].call(
        module.exports,
        localRequire,
        module,
        module.exports,
        globalObject
      );
    }

    return cache[name].exports;

    function localRequire(x) {
      var res = localRequire.resolve(x);
      return res === false ? {} : newRequire(res);
    }

    function resolve(x) {
      var id = modules[name][1][x];
      return id != null ? id : x;
    }
  }

  function Module(moduleName) {
    this.id = moduleName;
    this.bundle = newRequire;
    this.exports = {};
  }

  newRequire.isParcelRequire = true;
  newRequire.Module = Module;
  newRequire.modules = modules;
  newRequire.cache = cache;
  newRequire.parent = previousRequire;
  newRequire.register = function (id, exports) {
    modules[id] = [
      function (require, module) {
        module.exports = exports;
      },
      {},
    ];
  };

  Object.defineProperty(newRequire, 'root', {
    get: function () {
      return globalObject[parcelRequireName];
    },
  });

  globalObject[parcelRequireName] = newRequire;

  for (var i = 0; i < entry.length; i++) {
    newRequire(entry[i]);
  }

  if (mainEntry) {
    // Expose entry point to Node, AMD or browser globals
    // Based on https://github.com/ForbesLindesay/umd/blob/master/template.js
    var mainExports = newRequire(mainEntry);

    // CommonJS
    if (typeof exports === 'object' && typeof module !== 'undefined') {
      module.exports = mainExports;

      // RequireJS
    } else if (typeof define === 'function' && define.amd) {
      define(function () {
        return mainExports;
      });

      // <script>
    } else if (globalName) {
      this[globalName] = mainExports;
    }
  }
})({"e6PKT":[function(require,module,exports,__globalThis) {
///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
var parcelHelpers = require("@parcel/transformer-js/src/esmodule-helpers.js");
var _workflowModels = require("./workflow-models");
var _activityPicker = require("./activity-picker");
var _workflowUrlGenerator = require("./workflow-url-generator");
var _workflowCanvas = require("./workflow-canvas");
var _workflowCanvasDefault = parcelHelpers.interopDefault(_workflowCanvas);
// TODO: Re-implement this using a MVVM approach.
class WorkflowEditor extends (0, _workflowCanvasDefault.default) {
    constructor(container, workflowType, deleteActivityPrompt, localId, loadLocalState){
        super(container, workflowType), this.container = container, this.workflowType = workflowType, this.deleteActivityPrompt = deleteActivityPrompt, this.localId = localId, this.getState = ()=>{
            const $allActivityElements = $(this.container).find(".activity");
            const workflow = {
                id: this.workflowType.id,
                activities: [],
                transitions: [],
                removedActivities: this.workflowType.removedActivities
            };
            // Collect activities.
            for(let i = 0; i < $allActivityElements.length; i++){
                const $activityElement = $($allActivityElements[i]);
                const activityId = $activityElement.data("activity-id");
                const activityIsStart = $activityElement.data("activity-start");
                const activityIsEvent = $activityElement.data("activity-type") === "Event";
                const activityPosition = $activityElement.position();
                const activity = this.getActivity(activityId);
                workflow.activities.push({
                    id: activityId,
                    isStart: activityIsStart,
                    isEvent: activityIsEvent,
                    outcomes: activity.outcomes,
                    x: activityPosition.left,
                    y: activityPosition.top
                });
            }
            // Collect connections.
            const allConnections = this.jsPlumbInstance.getConnections();
            for(let i = 0; i < allConnections.length; i++){
                var connection = allConnections[i];
                var sourceEndpoint = connection.endpoints[0];
                var sourceOutcomeName = sourceEndpoint.getParameters().outcome.name;
                var sourceActivityId = $(connection.source).data("activity-id");
                var destinationActivityId = $(connection.target).data("activity-id");
                workflow.transitions.push({
                    sourceActivityId: sourceActivityId,
                    destinationActivityId: destinationActivityId,
                    sourceOutcomeName: sourceOutcomeName
                });
            }
            return workflow;
        }, this.serialize = ()=>{
            const workflow = this.getState();
            return JSON.stringify(workflow);
        }, this.saveLocalState = ()=>{
            sessionStorage[this.localId] = this.serialize();
        }, this.loadLocalState = ()=>{
            return JSON.parse(sessionStorage[this.localId]);
        };
        const self = this;
        jsPlumb.ready(()=>{
            jsPlumb.importDefaults(this.getDefaults());
            const plumber = this.createJsPlumbInstance();
            // Listen for new connections.
            plumber.bind("connection", function(connInfo, originalEvent) {
                const connection = connInfo.connection;
                const outcome = connection.getParameters().outcome;
                const label = connection.getOverlay("label");
                label.setLabel(outcome.displayName);
            });
            let activityElements = this.getActivityElements();
            var areEqualOutcomes = function(outcomes1, outcomes2) {
                if (outcomes1.length != outcomes2.length) return false;
                for(let i = 0; i < outcomes1.length; i++){
                    const outcome1 = outcomes1[i];
                    const outcome2 = outcomes2[i];
                    if (outcome1.name != outcome2.name || outcome1.displayName != outcome2.displayName) return false;
                }
                return true;
            };
            // Suspend drawing and initialize.
            plumber.batch(()=>{
                var serverworkflowType = this.workflowType;
                var workflowId = this.workflowType.id;
                if (loadLocalState) {
                    const localState = this.loadLocalState();
                    if (localState) this.workflowType = localState;
                }
                activityElements.each((_, activityElement)=>{
                    const $activityElement = $(activityElement);
                    const activityId = $activityElement.data("activity-id");
                    const isDeleted = this.workflowType.removedActivities.indexOf(activityId) > -1;
                    if (isDeleted) {
                        $activityElement.remove();
                        return;
                    }
                    let activity = this.getActivity(activityId);
                    const serverActivity = this.getActivity(activityId, serverworkflowType.activities);
                    // Update the activity's visual state.
                    if (loadLocalState) {
                        if (activity == null) {
                            // This is a newly added activity not yet added to local state.
                            activity = serverActivity;
                            this.workflowType.activities.push(activity);
                            activity.x = 50;
                            activity.y = 50;
                        } else {
                            // The available outcomes might have changed when editing an activity,
                            // so we need to check for that and update the client's activity outcomes if so.
                            const sameOutcomes = areEqualOutcomes(activity.outcomes, serverActivity.outcomes);
                            if (!sameOutcomes) activity.outcomes = serverActivity.outcomes;
                            $activityElement.css({
                                left: activity.x,
                                top: activity.y
                            }).toggleClass("activity-start", activity.isStart).data("activity-start", activity.isStart);
                        }
                    }
                    // Make the activity draggable.
                    plumber.draggable(activityElement, {
                        grid: [
                            10,
                            10
                        ],
                        containment: true,
                        start: (args)=>{
                            this.dragStart = {
                                left: args.e.screenX,
                                top: args.e.screenY
                            };
                        },
                        stop: (args)=>{
                            this.hasDragged = this.dragStart.left != args.e.screenX || this.dragStart.top != args.e.screenY;
                            this.updateCanvasHeight();
                        }
                    });
                    // Configure the activity as a target.
                    plumber.makeTarget(activityElement, {
                        dropOptions: {
                            hoverClass: "hover"
                        },
                        anchor: "Continuous",
                        endpoint: [
                            "Blank",
                            {
                                radius: 8
                            }
                        ]
                    });
                    // Add source endpoints.
                    for (let outcome of activity.outcomes){
                        const sourceEndpointOptions = this.getSourceEndpointOptions(activity, outcome);
                        plumber.addEndpoint(activityElement, {
                            connectorOverlays: [
                                [
                                    "Label",
                                    {
                                        label: outcome.displayName,
                                        cssClass: "connection-label"
                                    }
                                ]
                            ]
                        }, sourceEndpointOptions);
                    }
                });
                // Connect activities.
                this.updateConnections(plumber);
                // Re-query the activity elements.
                activityElements = this.getActivityElements();
                // Make all activity elements visible.
                activityElements.show();
                this.updateCanvasHeight();
            });
            // Initialize popovers.
            activityElements.each((_, item)=>{
                var activityElement = $(item);
                activityElement.popover({
                    trigger: "manual",
                    html: true,
                    content: ()=>{
                        const $content = activityElement.find(".activity-commands").clone();
                        const startButton = $content.find(".activity-start-action");
                        const isStart = activityElement.data("activity-start") === true;
                        const activityId = activityElement.data("activity-id");
                        startButton.attr("aria-pressed", activityElement.data("activity-start"));
                        startButton.toggleClass("active", isStart);
                        $content.on("click", ".activity-start-action", (e)=>{
                            e.preventDefault();
                            const button = $(e.currentTarget);
                            let isStart = button.is(".active");
                            if (isStart) {
                                button.removeClass("active");
                                isStart = false;
                            } else {
                                button.addClass("active");
                                isStart = true;
                            }
                            activityElement.data("activity-start", isStart);
                            activityElement.toggleClass("activity-start", isStart);
                        });
                        $content.on("click", ".activity-delete-action", (e)=>{
                            e.preventDefault();
                            // TODO: The prompts are really annoying. Consider showing some sort of small message balloon somewhere to undo the action instead.
                            //if (!confirm(self.deleteActivityPrompt)) {
                            //    return;
                            //}
                            self.workflowType.removedActivities.push(activityId);
                            plumber.remove(activityElement);
                            activityElement.popover("dispose");
                        });
                        $content.on("click", "[data-persist-workflow]", (e)=>{
                            self.saveLocalState();
                        });
                        return $content.get(0);
                    }
                });
            });
            $(container).on("click", ".activity", (e)=>{
                if (this.hasDragged) {
                    this.hasDragged = false;
                    return;
                }
                // if any other popovers are visible, hide them
                if (this.isPopoverVisible) activityElements.popover("hide");
                const sender = $(e.currentTarget);
                sender.popover("show");
                // handle clicking on the popover itself.
                $(".popover").off("click").on("click", (e2)=>{
                    e2.stopPropagation();
                });
                e.stopPropagation();
                this.isPopoverVisible = true;
            });
            $(container).on("dblclick", ".activity", (e)=>{
                const sender = $(e.currentTarget);
                const hasEditor = sender.data("activity-has-editor");
                if (hasEditor) {
                    this.saveLocalState();
                    sender.find(".activity-edit-action").get(0).click();
                }
            });
            // Hide all popovers when clicking anywhere but on an activity.
            $("body").on("click", (e)=>{
                activityElements.popover("hide");
                this.isPopoverVisible = false;
            });
            // Save local changes if the event target has the 'data-persist-workflow' attribute.
            $("body").on("click", "[data-persist-workflow]", (e)=>{
                this.saveLocalState();
            });
            this.jsPlumbInstance = plumber;
        });
    }
}
$.fn.workflowEditor = function() {
    this.each((index, element)=>{
        var $element = $(element);
        var workflowType = $element.data("workflow-type");
        var deleteActivityPrompt = $element.data("workflow-delete-activity-prompt");
        var localId = $element.data("workflow-local-id");
        var loadLocalState = $element.data("workflow-load-local-state");
        workflowType.removedActivities = workflowType.removedActivities || [];
        $element.data("workflowEditor", new WorkflowEditor(element, workflowType, deleteActivityPrompt, localId, loadLocalState));
    });
    return this;
};
$(document).ready(function() {
    const workflowEditor = $(".workflow-canvas").workflowEditor().data("workflowEditor");
    $("#workflowEditorForm").on("submit", (s, e)=>{
        const state = workflowEditor.serialize();
        $("#workflowStateInput").val(state);
    });
});

},{"./workflow-models":"dLsnn","./activity-picker":"bUrDU","./workflow-url-generator":"gXogA","./workflow-canvas":"6iROe","@parcel/transformer-js/src/esmodule-helpers.js":"bFOQM"}],"dLsnn":[function(require,module,exports,__globalThis) {

},{}],"bUrDU":[function(require,module,exports,__globalThis) {
///<reference path='../Lib/jquery/typings.d.ts' />
var applyFilter = function(category, q) {
    const type = $('.modal-activities').data('activity-type');
    category = category || $('.activity-picker-categories .nav-link.active').attr('href').substr(1);
    q = q || $('.modal-activities input[type=search]').val();
    const $cards = $('.activity.col').show();
    // Remove activities whoes type doesn't match the configured activity type.
    $cards.filter((i, el)=>{
        return $(el).data('activity-type') != type;
    }).hide();
    if (q.length > 0) // Remove activities whose title doesn't match the query.
    $cards.filter((i, el)=>{
        return $(el).find('.card-title').text().toLowerCase().indexOf(q.toLowerCase()) < 0 && q && q.length > 0;
    }).hide();
    else // Remove activities whose category doesn't match the selected one.
    $cards.filter((i, el)=>{
        return $(el).data('category').toLowerCase() != category.toLowerCase() && category.toLowerCase() != 'all';
    }).hide();
    // Show or hide categories based on whether there are any available activities.
    $('.activity-picker-categories [data-category]').each((i, el)=>{
        const categoryListItem = $(el);
        const category = categoryListItem.data('category');
        // Count number of activities within this category and for the specified activity type (Event or Task).
        const activityCount = $(`.activity.col[data-category='${category}'][data-activity-type='${type}']`).length;
        activityCount == 0 ? categoryListItem.hide() : categoryListItem.show();
    });
};
$(()=>{
    $('.activity-picker-categories').on('click', '.nav-link', (e)=>{
        applyFilter($(e.target).attr('href').substr(1), null);
    });
    $('.modal-activities input[type=search]').on('keyup', (e)=>{
        applyFilter(null, $(e.target).val());
    });
    $('#activity-picker').on('show.bs.modal', function(event) {
        var modalEvent = event;
        var button = $(modalEvent.relatedTarget); // Button that triggered the modal.
        var title = button.data('picker-title');
        var type = button.data('activity-type');
        var modal = $(this);
        modal.find('[href="#all"]').click();
        modal.find('.modal-title').text(title);
        modal.data('activity-type', type);
        applyFilter(null, null);
    });
});

},{}],"gXogA":[function(require,module,exports,__globalThis) {
///<reference path="../Lib/jquery/typings.d.ts" />
$(()=>{
    const generateWorkflowUrl = function() {
        const workflowTypeId = $('[data-workflow-type-id]').data('workflow-type-id');
        const activityId = $('[data-activity-id]').data('activity-id');
        var tokenLifeSpan = $('#token-lifespan').val();
        const generateUrl = $('[data-generate-url]').data('generate-url') + `?workflowTypeId=${workflowTypeId}&activityId=${activityId}&tokenLifeSpan=${tokenLifeSpan}`;
        const antiforgeryHeaderName = $('[data-antiforgery-header-name]').data('antiforgery-header-name');
        const antiforgeryToken = $('[data-antiforgery-token]').data('antiforgery-token');
        const headers = {};
        headers[antiforgeryHeaderName] = antiforgeryToken;
        $.post({
            url: generateUrl,
            headers: headers
        }).done((url)=>{
            $('#workflow-url-text').val(url);
        });
    };
    $('#generate-url-button').on('click', (e)=>{
        generateWorkflowUrl();
    });
    if ($('#workflow-url-text').val() == '') generateWorkflowUrl();
});

},{}],"6iROe":[function(require,module,exports,__globalThis) {
///<reference path='../Lib/jquery/typings.d.ts' />
///<reference path='../Lib/jsplumb/typings.d.ts' />
var parcelHelpers = require("@parcel/transformer-js/src/esmodule-helpers.js");
parcelHelpers.defineInteropFlag(exports);
var _workflowModels = require("./workflow-models");
class WorkflowCanvas {
    constructor(container, workflowType){
        this.container = container;
        this.workflowType = workflowType;
        this.minCanvasHeight = 400;
        this.getActivityElements = ()=>{
            return $(this.container).find('.activity');
        };
        this.getDefaults = ()=>{
            return {
                Anchor: "Continuous",
                DragOptions: {
                    cursor: 'pointer',
                    zIndex: 2000
                },
                EndpointStyles: [
                    {
                        fillStyle: '#225588'
                    }
                ],
                Endpoints: [
                    [
                        "Dot",
                        {
                            radius: 7
                        }
                    ],
                    [
                        "Blank"
                    ]
                ],
                ConnectionOverlays: [
                    [
                        "Arrow",
                        {
                            width: 12,
                            length: 12,
                            location: -5
                        }
                    ]
                ],
                ConnectorZIndex: 5
            };
        };
        this.createJsPlumbInstance = ()=>{
            return jsPlumb.getInstance({
                DragOptions: {
                    cursor: 'pointer',
                    zIndex: 2000
                },
                ConnectionOverlays: [
                    [
                        'Arrow',
                        {
                            location: 1,
                            visible: true,
                            width: 11,
                            length: 11
                        }
                    ],
                    [
                        'Label',
                        {
                            location: 0.5,
                            id: 'label',
                            cssClass: 'connection-label'
                        }
                    ]
                ],
                Container: this.container
            });
        };
        this.getEndpointColor = (activity)=>{
            return activity.isBlocking || activity.isStart ? '#7ab02c' : activity.isEvent ? '#3a8acd' : '#7ab02c';
        };
        this.getSourceEndpointOptions = (activity, outcome)=>{
            // The definition of source endpoints.
            const paintColor = this.getEndpointColor(activity);
            return {
                endpoint: 'Dot',
                anchor: 'Continuous',
                paintStyle: {
                    stroke: paintColor,
                    fill: paintColor,
                    radius: 7,
                    strokeWidth: 1
                },
                isSource: true,
                connector: [
                    'Flowchart',
                    {
                        stub: [
                            40,
                            60
                        ],
                        gap: 0,
                        cornerRadius: 5,
                        alwaysRespectStubs: true
                    }
                ],
                connectorStyle: {
                    strokeWidth: 2,
                    stroke: '#999999',
                    joinstyle: 'round',
                    outlineStroke: 'white',
                    outlineWidth: 2
                },
                hoverPaintStyle: {
                    fill: '#216477',
                    stroke: '#216477'
                },
                connectorHoverStyle: {
                    strokeWidth: 3,
                    stroke: '#216477',
                    outlineWidth: 5,
                    outlineStroke: 'white'
                },
                connectorOverlays: [
                    [
                        'Label',
                        {
                            location: [
                                3,
                                -1.5
                            ],
                            cssClass: 'endpointSourceLabel'
                        }
                    ]
                ],
                dragOptions: {},
                uuid: `${activity.id}-${outcome.name}`,
                parameters: {
                    outcome: outcome
                }
            };
        };
        this.getActivity = function(id, activities = null) {
            if (!activities) activities = this.workflowType.activities;
            return $.grep(activities, (x)=>x.id === id)[0];
        };
        this.updateConnections = (plumber)=>{
            var workflowId = this.workflowType.id;
            // Connect activities.
            for (let transitionModel of this.workflowType.transitions){
                const sourceEndpointUuid = `${transitionModel.sourceActivityId}-${transitionModel.sourceOutcomeName}`;
                const sourceEndpoint = plumber.getEndpoint(sourceEndpointUuid);
                const destinationElementId = `activity-${workflowId}-${transitionModel.destinationActivityId}`;
                plumber.connect({
                    source: sourceEndpoint,
                    target: destinationElementId
                });
            }
        };
        this.updateCanvasHeight = function() {
            const $container = $(this.container);
            // Get the activity element with the highest Y coordinate.
            const $activityElements = $container.find(".activity");
            let currentElementTop = 0;
            let currentActivityHeight = 0;
            for (let activityElement of $activityElements.toArray()){
                const $activityElement = $(activityElement);
                const top = $activityElement.position().top;
                if (top > currentElementTop) {
                    currentElementTop = top;
                    currentActivityHeight = $activityElement.height();
                }
            }
            let newCanvasHeight = currentElementTop + currentActivityHeight;
            const elementBottom = currentElementTop + currentActivityHeight;
            const stretchValue = 100;
            if (newCanvasHeight - elementBottom <= stretchValue) newCanvasHeight += stretchValue;
            $container.height(Math.max(this.minCanvasHeight, newCanvasHeight));
        };
    }
}
exports.default = WorkflowCanvas;

},{"./workflow-models":"dLsnn","@parcel/transformer-js/src/esmodule-helpers.js":"bFOQM"}],"bFOQM":[function(require,module,exports,__globalThis) {
exports.interopDefault = function(a) {
    return a && a.__esModule ? a : {
        default: a
    };
};
exports.defineInteropFlag = function(a) {
    Object.defineProperty(a, '__esModule', {
        value: true
    });
};
exports.exportAll = function(source, dest) {
    Object.keys(source).forEach(function(key) {
        if (key === 'default' || key === '__esModule' || Object.prototype.hasOwnProperty.call(dest, key)) return;
        Object.defineProperty(dest, key, {
            enumerable: true,
            get: function() {
                return source[key];
            }
        });
    });
    return dest;
};
exports.export = function(dest, destName, get) {
    Object.defineProperty(dest, destName, {
        enumerable: true,
        get: get
    });
};

},{}]},["e6PKT"], "e6PKT", "parcelRequire94c2")

