﻿namespace FileFlows.Client.Components.Dashboard
{
    using FileFlows.Shared;
    using FileFlows.Shared.Helpers;
    using Microsoft.AspNetCore.Components;
    using Microsoft.JSInterop;
    using System;
    using System.Threading.Tasks;
    using System.Timers;

    public partial class ShrinkageBar:ComponentBase
    {
        [CascadingParameter] Pages.Dashboard Dashboard { get; set; }
        private string Uid = Guid.NewGuid().ToString();


        private Dictionary<string, ShrinkageData> Data;
        private Timer AutoRefreshTimer;

        private string lblTotal, lblOther, lblShrinkageTitle;

        private bool HasRendered = false;

        protected override async Task OnInitializedAsync()
        {
            lblShrinkageTitle = Translater.Instant("Pages.Dashboard.Labels.ShrinkageTitle");
            lblTotal = Translater.Instant("Labels.Total");
            lblOther = Translater.Instant("Labels.Other");

            Dashboard.OnDisposing += Dispose;

            AutoRefreshTimer = new Timer();
            AutoRefreshTimer.Elapsed += AutoRefreshTimerElapsed;
            AutoRefreshTimer.Interval = 60_000;
            AutoRefreshTimer.AutoReset = true;
            AutoRefreshTimer.Start();
            await Refresh();
        }


        public void Dispose()
        {
            Dashboard.OnDisposing -= Dispose;
            if (AutoRefreshTimer != null)
            {
                AutoRefreshTimer.Stop();
                AutoRefreshTimer.Elapsed -= AutoRefreshTimerElapsed;
                AutoRefreshTimer.Dispose();
                AutoRefreshTimer = null;
            }
        }
        void AutoRefreshTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _ = Refresh();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            this.HasRendered = true;
            base.OnAfterRender(firstRender);
        }

        private async Task WaitUntilHasRendered()
        {
            while (this.HasRendered == false)
                await Task.Delay(50);
        }


        private async Task Refresh()
        {
#if (DEMO)
            if (Data?.Any() != true)
            {
                Random random = new Random(DateTime.Now.Millisecond);

                Data = Enumerable.Range(1, 5).ToDictionary(x => "Library " + x, x => new ShrinkageData
                {
                    FinalSize = random.Next(1_000, 10_000) * 10_000_000L,
                    OriginalSize = random.Next(19_000, 50_000) * 10_000_000L,
                    Items = random.Next(1000, 10_000)
                });
            }
#else
            var result = await HttpHelper.Get<Dictionary<string,ShrinkageData>>("/api/library-file/shrinkage-groups");
            if (result.Success)
            {
                Data = result.Data ?? new ();
            }
            else if (Data == null)
                Data = new ();
#endif
        }

    }
}
