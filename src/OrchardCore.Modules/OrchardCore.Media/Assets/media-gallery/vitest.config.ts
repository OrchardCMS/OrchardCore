//import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    define: {
      "process.env.NODE_ENV": JSON.stringify("test"),
    },
    test: {
      globals: true,
      environment: 'jsdom',
      exclude: [...configDefaults.exclude, 'e2e/*'],
      //root: fileURLToPath(new URL('./', import.meta.url)),
      reporters: ['default','junit'],
      outputFile: './testing/vitest-results.xml',
      coverage: {
        reporter: ['cobertura', 'html'], // Cobertura for Azure DevOps, HTML for human-readable format
        include: ['src/**/*.{ts,vue}'],
        exclude: [
          // Test files
          'src/**/__tests__/**',
          'src/**/*.spec.ts',
          'src/**/*.test.ts',
          // Config files
          '*.config.ts',
          '*.config.js',
          '*.cjs',
          // Stub/no-op files with no testable logic
          'src/main.ts',
          'src/components/ModalFileProcess.vue',
          'src/services/FileProcessModalService.ts',
        ],
      },
    }
  })
)
