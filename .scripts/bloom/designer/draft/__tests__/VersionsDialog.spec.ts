import { afterEach, describe, expect, it } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import VersionsDialog from "../VersionsDialog.vue";
import type { VersionsData } from "../../core/types";

interface Version {
    versionId: string;
    number: number;
    isPublished: boolean;
    publishedBy: string;
}

const data: VersionsData<Version> = {
    draft: { modifiedUtc: "2026-10-01T10:00:00Z", modifiedBy: "admin" },
    versions: [
        { versionId: "v2", number: 2, isPublished: true, publishedBy: "admin" },
        { versionId: "v1", number: 1, isPublished: false, publishedBy: "editor" },
    ],
};

describe("VersionsDialog", () => {
    afterEach(() => {
        document.body.innerHTML = "";
    });

    it("render_Versions_ListsThemWithTheDesignerDescription", async () => {
        mount(VersionsDialog, {
            props: { load: () => Promise.resolve(data), describe: (version: Version) => `By ${version.publishedBy}` },
            attachTo: document.body,
        });
        await flushPromises();

        expect(document.querySelector("[data-cy='versions-draft']")).not.toBeNull();
        expect(document.querySelector("[data-cy='version-2']")!.textContent).toContain("By admin");
        expect(document.querySelector("[data-cy='version-2'] [data-cy='version-published']")).not.toBeNull();
        // By default, every version but the published one can be restored.
        expect(document.querySelector("[data-cy='version-2'] [data-cy='version-restore']")).toBeNull();
        expect(document.querySelector("[data-cy='version-1'] [data-cy='version-restore']")).not.toBeNull();
    });

    it("click_Restore_EmitsTheVersion", async () => {
        const wrapper = mount(VersionsDialog, {
            props: { load: () => Promise.resolve(data), describe: () => "", canRestore: () => true },
            attachTo: document.body,
        });
        await flushPromises();

        document.querySelector<HTMLButtonElement>("[data-cy='version-2'] [data-cy='version-restore']")!.click();

        expect(wrapper.emitted("restore")).toEqual([[data.versions[0]]]);
    });

    it("render_LoadFailed_SaysSo", async () => {
        mount(VersionsDialog, {
            props: { load: () => Promise.reject(new Error("Failed")), describe: () => "" },
            attachTo: document.body,
        });
        await flushPromises();

        expect(document.querySelector("[role='alert']")!.textContent).toContain("could not be loaded");
    });
});
