/**
 * A reversible change of the designer graph.
 */
export interface Command {
    readonly label: string;
    apply(): void;
    revert(): void;
    /**
     * Consecutive commands with the same non-empty key become one history entry, for example every step of
     * a node drag.
     */
    readonly coalesceKey?: string;
    /**
     * Absorbs `next`, which is already applied, into this command. Returns false to keep two entries.
     */
    absorb?(next: Command): boolean;
}

export const HISTORY_LIMIT = 100;

/**
 * Command-pattern undo/redo, capped at HISTORY_LIMIT entries (the oldest entries are dropped).
 */
export class History {
    private readonly limit: number;
    private readonly undoStack: Command[] = [];
    private readonly redoStack: Command[] = [];
    private readonly listeners = new Set<() => void>();

    constructor(limit = HISTORY_LIMIT) {
        this.limit = limit;
    }

    get canUndo() {
        return this.undoStack.length > 0;
    }

    get canRedo() {
        return this.redoStack.length > 0;
    }

    get size() {
        return this.undoStack.length;
    }

    /**
     * Applies a command and records it.
     */
    execute(command: Command) {
        command.apply();
        this.record(command);
    }

    /**
     * Records a command whose effect is already applied.
     */
    record(command: Command) {
        const last = this.undoStack[this.undoStack.length - 1];

        this.redoStack.length = 0;

        if (command.coalesceKey && last?.coalesceKey === command.coalesceKey && last.absorb?.(command)) {
            this.notify();

            return;
        }

        this.undoStack.push(command);

        if (this.undoStack.length > this.limit) {
            this.undoStack.splice(0, this.undoStack.length - this.limit);
        }

        this.notify();
    }

    undo(): Command | undefined {
        const command = this.undoStack.pop();

        if (command) {
            command.revert();
            this.redoStack.push(command);
            this.notify();
        }

        return command;
    }

    redo(): Command | undefined {
        const command = this.redoStack.pop();

        if (command) {
            command.apply();
            this.undoStack.push(command);
            this.notify();
        }

        return command;
    }

    /**
     * Marks a change that can't be reverted locally, such as an activity editor apply (the properties
     * changed on the server). Undo never goes back past it, so the entries before it are dropped.
     */
    markBoundary() {
        this.clear();
    }

    clear() {
        this.undoStack.length = 0;
        this.redoStack.length = 0;
        this.notify();
    }

    subscribe(listener: () => void) {
        this.listeners.add(listener);

        return () => this.listeners.delete(listener);
    }

    private notify() {
        this.listeners.forEach((listener) => listener());
    }
}
