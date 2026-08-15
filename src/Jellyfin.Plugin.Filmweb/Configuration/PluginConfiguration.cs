using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Filmweb.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public bool PreferPolishTitle { get; set; } = true;

    public bool FetchPeople { get; set; } = true;

    public bool FetchRatings { get; set; } = true;
}
