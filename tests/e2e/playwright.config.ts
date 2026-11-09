import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './specs',
  fullyParallel: false,
  workers: 1,
  timeout: 60000,
  expect: { timeout: 30000 },
  use: {
    baseURL: 'http://localhost:5219',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: {
    command: 'dotnet run --project ../../MyCorner',
    url: 'http://localhost:5219',
    reuseExistingServer: true,
    timeout: 120000,
  },
});
