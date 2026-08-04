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
      reporters: ['default', 'junit'],
      outputFile: './testing/vitest-results.xml',
      coverage: {
        reporter: ['cobertura', 'html'],
        include: ['src/**/*.{ts,vue}'],
        exclude: [
          'src/**/__tests__/**',
          'src/**/*.spec.ts',
          'src/**/*.test.ts',
          '*.config.ts',
          '*.config.js',
          '*.cjs',
          'src/env.d.ts',
          'src/interfaces/**',
        ],
      },
    }
  })
)
