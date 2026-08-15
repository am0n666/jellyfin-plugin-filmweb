using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Filmweb.Providers;

public class FilmwebExternalId : IExternalId
{
    public string ProviderName => "Filmweb";
    public string Key => "Filmweb";
    public ExternalIdMediaType? Type => null;
    public string UrlFormatString => "https://www.filmweb.pl/film/-{0}";
    public bool Supports(IHasProviderIds item) => item is Movie or Series;
}
