import { test, expect } from '@playwright/test';

test.describe('Console', () => {
  test.beforeEach(async ({ page }) => {
    const errors: string[] = [];
    (page as any).__consoleErrors = errors;
    page.on('console', (msg) => {
      if (msg.type() === 'error') errors.push(msg.text());
    });
    page.on('pageerror', (err) => errors.push(err.message));

    await page.addInitScript(() => {
      (window as any).__openedUrls = [];
      (window as any).open = (url: any) => {
        (window as any).__openedUrls.push(String(url));
        return null;
      };
      (window as any).openInNewTab = (url: any) => {
        (window as any).__openedUrls.push(String(url));
      };
    });

    await page.goto('/');
    await expect(page.getByTestId('location-header')).toBeVisible({ timeout: 60000 });
    await expect(page.getByTestId('location-header')).toHaveText('TARDIS Console Room');
  });

  async function submit(page: any, command: string) {
    await page.getByTestId('command-input').fill(command);
    await page.getByTestId('submit-button').click();
  }

  function output(page: any) {
    return page.getByTestId('console-output');
  }

  function openedUrls(page: any) {
    return page.evaluate(() => (window as any).__openedUrls ?? []);
  }

  function assertNoConsoleErrors(page: any) {
    expect((page as any).__consoleErrors).toEqual([]);
  }

  async function gotoWorkshop(page: any) {
    await submit(page, 'go west');
    await expect(page.getByTestId('location-header')).toHaveText('Project Workshop');
  }

  test('S1 initial page load', async ({ page }) => {
    await expect(page.getByTestId('location-header')).toHaveText('TARDIS Console Room');
    await expect(output(page)).toContainText('TARDIS');
    await expect(output(page)).toContainText('Available exits');
    await expect(output(page)).toContainText('workshop (west)');
    assertNoConsoleErrors(page);
  });

  test('S2 LinkedIn opens new window', async ({ page }) => {
    const link = page.getByTestId('linkedin-link');
    await expect(link).toHaveAttribute('target', '_blank');
    await expect(link).toHaveAttribute('href', /linkedin\.com\/in\/christa-bridges/);

    const popupPromise = page.waitForEvent('popup');
    await link.click();
    const popup = await popupPromise;
    await popup.close();

    assertNoConsoleErrors(page);
  });

  test('S3 help command', async ({ page }) => {
    await submit(page, 'help');
    await expect(output(page)).toContainText('> help');
    await expect(output(page)).toContainText('help: Show this help text');
    await expect(output(page)).toContainText('Location-specific commands');
    await expect(output(page)).toContainText('goto workshop');
    assertNoConsoleErrors(page);
  });

  test('S4 unknown command', async ({ page }) => {
    await submit(page, 'asdfghjkl');
    await expect(output(page)).toContainText('> asdfghjkl');
    await expect(output(page)).toContainText("Sorry, I'm not sure what you mean");
    await expect(output(page)).toContainText("Try 'help'");
    assertNoConsoleErrors(page);
  });

  test('S5 rickroll opens YouTube', async ({ page }) => {
    await submit(page, 'rickroll');
    await expect(output(page)).toContainText('> rickroll');
    await expect(output(page)).toContainText("Of course! I'd never let you down");
    const urls = await openedUrls(page);
    expect(urls).toContain('https://www.youtube.com/watch?v=dQw4w9WgXcQ');
    assertNoConsoleErrors(page);
  });

  test('S6 go west -> workshop', async ({ page }) => {
    await submit(page, 'go west');
    await expect(output(page)).toContainText('> go west');
    await expect(page.getByTestId('location-header')).toHaveText('Project Workshop');
    await expect(output(page)).toContainText('Key projects');
    await expect(output(page)).toContainText('personal website');
    assertNoConsoleErrors(page);
  });

  test('S7 go east -> console room', async ({ page }) => {
    await gotoWorkshop(page);
    await submit(page, 'go east');
    await expect(output(page)).toContainText('> go east');
    await expect(page.getByTestId('location-header')).toHaveText('TARDIS Console Room');
    assertNoConsoleErrors(page);
  });

  test('S8 view personal website', async ({ page }) => {
    await gotoWorkshop(page);
    await submit(page, 'view personal website');
    await expect(output(page)).toContainText('> view personal website');
    await expect(output(page)).toContainText("The site you're looking at now");
    await expect(output(page).locator('a[href="https://github.com/cBridges851/cBridges851.github.io/"]')).toBeVisible();
    assertNoConsoleErrors(page);
  });

  test('S9 empty command', async ({ page }) => {
    await submit(page, '');
    await expect(output(page)).toContainText("Sorry, I'm not sure what you mean");
    assertNoConsoleErrors(page);
  });

  test('S10 go with no destination', async ({ page }) => {
    await submit(page, 'go');
    await expect(output(page)).toContainText('Go where?');
    assertNoConsoleErrors(page);
  });

  test('S11 view with no project', async ({ page }) => {
    await gotoWorkshop(page);
    await submit(page, 'view');
    await expect(output(page)).toContainText('View what?');
    assertNoConsoleErrors(page);
  });
});
