using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Plugin.Filmweb.Providers;

namespace Jellyfin.Plugin.Filmweb;

public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<FilmwebMovieProvider>();
        serviceCollection.AddSingleton<FilmwebSeriesProvider>();
        serviceCollection.AddSingleton<FilmwebImageProvider>();
        serviceCollection.AddSingleton<FilmwebExternalId>();
    }
}
