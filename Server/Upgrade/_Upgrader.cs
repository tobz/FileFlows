﻿using FileFlows.Server.Controllers;
using FileFlows.Server.Helpers;
using FileFlows.Shared.Models;

namespace FileFlows.Server.Upgrade;

public class Upgrader
{
    public void Run(Settings settings)
    {
        var currentVersion = string.IsNullOrWhiteSpace(settings.Version) ? new Version() : Version.Parse(settings.Version);

        if(currentVersion <  new Version(0, 5, 0))
            new UpgradeStats().Run();
        if (currentVersion < new Version(0, 5, 3))
            new Upgrade0_5_3().Run();
        if (currentVersion < new Version(0, 6, 0))
            new Upgrade0_6_0().Run();
        if (currentVersion < new Version(0, 6, 1, 987))
            new Upgrade0_6_1().Run(settings);
        if (currentVersion < new Version(0, 7, 0, 1120))
            new Upgrade0_7_0().Run(settings);

        // always check on default scripts
        new DefaultScripts().Run(settings);
        

        // save the settings
        if (settings.Version != Globals.Version)
        {
            settings.Version = Globals.Version;
            DbHelper.Update(settings).Wait();
        }
    }
}
