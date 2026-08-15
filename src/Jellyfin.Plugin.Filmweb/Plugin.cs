using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Filmweb.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Filmweb;

public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public static Plugin? Instance { get; private set; }

    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    public override string Name => "Filmweb";

    public override Guid Id => Guid.Parse("b1f5d22e-a34e-4f5a-939e-2fbbbc512a89");

    public override string Description => "Pobiera metadane, oceny i grafiki dla filmow i seriali z serwisu Filmweb.pl";

    public IEnumerable<PluginPageInfo> GetPages() => [];
}
