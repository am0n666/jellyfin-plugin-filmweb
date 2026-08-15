using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Filmweb.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

namespace Jellyfin.Plugin.Filmweb.Providers;

public class FilmwebMovieProvider : IRemoteMetadataProvider<Movie, MovieInfo>, IHasOrder
{
    private readonly FilmwebClient _client;

    public FilmwebMovieProvider(IHttpClientFactory httpClientFactory)
    {
        _client = new FilmwebClient(httpClientFactory.CreateClient());
    }

    public int Order => 1;
    public string Name => "Filmweb";

    public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
    {
        var result = new MetadataResult<Movie>();

        if (!info.TryGetProviderId("Filmweb", out var filmwebId))
        {
            var searchResults = await _client.SearchAsync(info.Name, info.Year, "film", cancellationToken).ConfigureAwait(false);
            if (searchResults.Count == 0)
            {
                return result;
            }
            filmwebId = searchResults[0].Id;
        }

        var details = await _client.GetDetailsAsync(filmwebId, "film", cancellationToken).ConfigureAwait(false);
        if (details == null)
        {
            return result;
        }

        var movie = new Movie
        {
            Name = details.Title,
            OriginalTitle = details.OriginalTitle,
            Overview = details.Overview,
            ProductionYear = details.Year,
            CommunityRating = details.Rating
        };

        foreach (var genre in details.Genres)
        {
            movie.AddGenre(genre);
        }

        result.Item = movie;
        result.HasMetadata = true;
        result.Item.SetProviderId("Filmweb", details.Id);

        foreach (var personName in details.Directors)
        {
            result.AddPerson(new PersonInfo
            {
                Name = personName,
                Type = PersonKind.Director
            });
        }

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

    public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
    {
        var list = new List<RemoteSearchResult>();
        var results = await _client.SearchAsync(searchInfo.Name, searchInfo.Year, "film", cancellationToken).ConfigureAwait(false);

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
