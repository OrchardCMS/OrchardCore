import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import PresenceList from "../PresenceList.vue";

describe("PresenceList", () => {
    it.each([
        ["Alice Smith", "AS"],
        ["😀 Smith", "😀S"],
        ["e\u0301mile Smith", "E\u0301S"],
        ["👩‍💻 Smith", "👩‍💻S"],
        ["🇺🇸 Smith", "🇺🇸S"],
        ["👍🏽", "👍🏽"],
    ])("uses whole graphemes for initials of %s", (userName, expected) => {
        const wrapper = mount(PresenceList, { props: { presence: [{ connectionId: "one", userId: "user", userName }] } });

        expect(wrapper.get(".wfd-presence-avatar").text()).toBe(expected);
        expect(wrapper.get(".wfd-presence-avatar").attributes("title")).toBe(userName);
        wrapper.unmount();
    });

    it("keeps one avatar per user and at most two initials", () => {
        const wrapper = mount(PresenceList, {
            props: {
                presence: [
                    { connectionId: "one", userId: "user", userName: "😀 Middle Last" },
                    { connectionId: "two", userId: "user", userName: "😀 Middle Last" },
                ],
            },
        });

        expect(wrapper.findAll(".wfd-presence-avatar")).toHaveLength(1);
        expect(wrapper.get(".wfd-presence-avatar").text()).toBe("😀M");
        wrapper.unmount();
    });
});
