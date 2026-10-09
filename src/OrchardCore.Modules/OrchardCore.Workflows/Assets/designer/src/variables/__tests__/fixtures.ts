import type { VariableDefinition, VariableType } from "../../api/types";

export const variableTypes: VariableType[] = [
    { name: "string", displayName: "Text", editor: "text" },
    { name: "number", displayName: "Number", editor: "number" },
    { name: "boolean", displayName: "Boolean", editor: "boolean" },
    { name: "datetime", displayName: "Date and time", editor: "datetime" },
    { name: "object", displayName: "Object", editor: "json" },
    { name: "any", displayName: "Any", editor: "json" },
    { name: "contentItem", displayName: "Content item", editor: "none" },
];

export const variables: VariableDefinition[] = [
    { name: "greeting", typeName: "string", defaultValue: "Hello", description: "Shown to the user" },
    { name: "attempts", typeName: "number", defaultValue: 0 },
];
