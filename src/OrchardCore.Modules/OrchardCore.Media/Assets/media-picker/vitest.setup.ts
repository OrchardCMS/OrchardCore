import { afterEach } from 'vitest'
import { enableAutoUnmount } from '@vue/test-utils'

// Unmount every wrapper after each test so component teardown hooks run. Without this a
// component that schedules a timer keeps it pending, and the callback can fire after the
// jsdom environment is torn down, failing the run with "document is not defined".
enableAutoUnmount(afterEach)
