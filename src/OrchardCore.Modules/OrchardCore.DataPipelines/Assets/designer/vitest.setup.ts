import { afterEach } from "vitest";
import { enableAutoUnmount } from "@vue/test-utils";

// Unmount every wrapper after each test so component teardown hooks run (timers, listeners).
enableAutoUnmount(afterEach);

// The designer keeps per-browser preferences (panel width, collapsed panes); each test starts without them.
afterEach(() => window.localStorage.clear());
