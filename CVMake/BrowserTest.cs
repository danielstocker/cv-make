using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Diagnostics;
using CVMake;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

public class BrowserTest
{

    internal bool CheckWithinSize(string url, int maxLength = 1060)
    {
        Task<int> length = getPageLength(url);
        length.Wait();

        return length.Result < maxLength;
    }

    internal int GetPageLength(string url) {
        Task<int> length = getPageLength(url);
        length.Wait();

        return length.Result;
    }

    private async Task<int> getPageLength(string url)
    {
        if(url.StartsWith("/")) {
            url = "file://" + url;
        }
        
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = null
        });

        var page = await browser.NewPageAsync();
        await page.GotoAsync(url);
        await Task.Delay(1250);

        await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
        await Task.Delay(250);
        var bodyHeight = await page.EvaluateAsync<int>("document.body.scrollHeight");

        await Task.Delay(250);

        await page.CloseAsync();
        await browser.CloseAsync();
        await browser.DisposeAsync();
        playwright.Dispose();

        return bodyHeight;
    }

    
}
