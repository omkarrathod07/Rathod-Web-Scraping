using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using RathodWebScraping.Data;
using System;
using System.Threading.Tasks;

namespace RathodWebScraping.Components.Pages
{
    public partial class Home : ComponentBase
    {
        [Inject] private IDbContextFactory<ChatDbContext> DbFactory { get; set; } = default!;

        protected int totalSessionsCount = 0;
        protected int scrapedDomainsCount = 4;

        protected override async Task OnInitializedAsync()
        {
            ThemeState.OnChange += StateHasChanged;

            await LoadDatabaseMetricsSummary();
        }

        private async Task LoadDatabaseMetricsSummary()
        {
            try
            {
                using var db = await DbFactory.CreateDbContextAsync();
                totalSessionsCount = await db.Sessions.CountAsync();
            }
            catch
            {
                totalSessionsCount = 0;
            }
        }

        protected void NavigateTo(string destinationRoute)
        {
            NavigationManager.NavigateTo(destinationRoute);
        }

        public void Dispose()
        {
            ThemeState.OnChange -= StateHasChanged;
        }
    }
}