import { describe, expect, it, vi } from "vitest";
import { createApp } from "vue";

// Mock the NSwag-generated OpenApiClient so FileDataService can be constructed in tests.
vi.mock("@bloom/services/OpenApiClient", () => ({
  Client: vi.fn().mockImplementation(() => ({})),
  MoveMedias: vi.fn().mockImplementation((data: any) => data), // eslint-disable-line @typescript-eslint/no-explicit-any
  DirectoryTreeNodeDto: vi.fn(),
}));

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
