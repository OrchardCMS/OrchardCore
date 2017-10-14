///<reference path="./Types/jsplumb/index.d.ts" />

export class WorkflowEditor {
    constructor() {
        this.jsPlumbInstance = jsPlumb.getInstance();
        this.jsPlumbInstance.bind('ready', () => {
            
        });
    }

    private jsPlumbInstance: jsPlumbInstance;
}