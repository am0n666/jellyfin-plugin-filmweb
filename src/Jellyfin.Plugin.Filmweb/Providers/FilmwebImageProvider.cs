using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Filmweb.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Filmweb.Providers;

public class FilmwebImageProvider : IRemoteImageProvider, IHasOrder
{
    private readonly FilmwebClient _client;
    private readonly HttpClient _httpClient;

    public FilmwebImageProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _client = new FilmwebClient(_httpClient);
    }

    public string Name => "Filmweb";
    public int Order => 1;

    public bool Supports(BaseItem item) => item is Movie or Series;

    public IEnumerable<ImageType> GetSupportedImages(BaseItem item) => [ImageType.Primary];

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        var list = new List<RemoteImageInfo>();
        if (!item.TryGetProviderId("Filmweb", out var filmwebId))
        {
            return list;
        }

        var itemType = item is Series ? "serial" : "film";
        var details = await _client.GetDetailsAsync(filmwebId, itemType, cancellationToken).ConfigureAwait(false);
        if (details?.PosterUrl != null)
        {
            list.Add(new RemoteImageInfo
            {
                ProviderName = Name,
                Type = ImageType.Primary,
                Url = details.PosterUrl
            });
        }

        return list;
    }

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return _httpClient.GetAsync(url, cancellationToken);
    }
}
