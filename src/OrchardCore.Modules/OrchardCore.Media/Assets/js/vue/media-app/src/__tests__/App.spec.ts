import { describe, expect, it } from "vitest";
import { createApp } from "vue";
import App from "../App.vue";

describe("app", () => {
  it("should create the Vue app", () => {
    const app = createApp(App);
    expect(app).toBeDefined();
  });

  it("should have the App component defined", () => {
    expect(App).toBeDefined();
  });
});
