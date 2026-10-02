import { afterEach } from "vitest";
import { enableAutoUnmount } from "@vue/test-utils";

// Unmount every wrapper after each test so component teardown hooks run (timers, listeners).
enableAutoUnmount(afterEach);
