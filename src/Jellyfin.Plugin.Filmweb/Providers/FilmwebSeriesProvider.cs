using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Plugin.Filmweb.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Filmweb.Providers;

public class FilmwebSeriesProvider : IRemoteMetadataProvider<Series, SeriesInfo>, IHasOrder
{
    private readonly FilmwebClient _client;

    public FilmwebSeriesProvider(IHttpClientFactory httpClientFactory)
    {
        _client = new FilmwebClient(httpClientFactory.CreateClient());
    }

    public int Order => 1;
    public string Name => "Filmweb";

    public async Task<MetadataResult<Series>> GetMetadata(SeriesInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Series>();

        if (!info.TryGetProviderId("Filmweb", out var filmwebId))
        {
            var searchResults = await _client.SearchAsync(info.Name, info.Year, "serial", cancellationToken).ConfigureAwait(false);
            if (searchResults.Count == 0)
            {
                return result;
            }
            filmwebId = searchResults[0].Id;
        }

        var details = await _client.GetDetailsAsync(filmwebId, "serial", cancellationToken).ConfigureAwait(false);
        if (details == null)
        {
            return result;
        }

        var series = new Series
        {
            Name = details.Title,
            OriginalTitle = details.OriginalTitle,
            Overview = details.Overview,
            ProductionYear = details.Year,
            CommunityRating = details.Rating
        };

        foreach (var genre in details.Genres)
        {
            series.AddGenre(genre);
        }

        result.Item = series;
        result.HasMetadata = true;
        result.Item.SetProviderId("Filmweb", details.Id);

        foreach (var personName in details.Actors)
        {
            result.AddPerson(new PersonInfo
            {
                Name = personName,
                Type = PersonKind.Actor
            });
        }

        return result;
    }

    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(SeriesInfo searchInfo, CancellationToken cancellationToken)
    {
        var list = new List<RemoteSearchResult>();
        var results = await _client.SearchAsync(searchInfo.Name, searchInfo.Year, "serial", cancellationToken).ConfigureAwait(false);

        foreach (var item in results)
        {
            var res = new RemoteSearchResult
            {
                Name = item.Title,
                ProductionYear = item.Year,
                SearchProviderName = Name
            };
            res.SetProviderId("Filmweb", item.Id);
            list.Add(res);
        }

        return list;
    }

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
