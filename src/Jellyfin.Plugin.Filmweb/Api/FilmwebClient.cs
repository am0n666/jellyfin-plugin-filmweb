using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace Jellyfin.Plugin.Filmweb.Api;

public class FilmwebClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.filmweb.pl";

    public FilmwebClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
        if (!_httpClient.DefaultRequestHeaders.Contains("Accept-Language"))
        {
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7");
        }
    }

    public async Task<List<FilmwebSearchResult>> SearchAsync(string query, int? year, string itemType, CancellationToken cancellationToken)
    {
        var results = new List<FilmwebSearchResult>();
        if (string.IsNullOrWhiteSpace(query))
        {
            return results;
        }

        var encoded = HttpUtility.UrlEncode(query);
        var searchUrl = $"{BaseUrl}/search?q={encoded}";

        string html;
        try
        {
            html = await _httpClient.GetStringAsync(searchUrl, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return results;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var links = doc.DocumentNode.SelectNodes("//a[contains(@href, '/film/') or contains(@href, '/serial/')]");
        if (links != null)
        {
            var seenIds = new HashSet<string>();
            foreach (var link in links)
            {
                var href = link.GetAttributeValue("href", "");
                if (string.IsNullOrEmpty(href) || href.Contains("/ranking/") || href.Contains("/vod/"))
                {
                    continue;
                }

                var title = HttpUtility.HtmlDecode(link.InnerText).Trim();
                if (string.IsNullOrWhiteSpace(title) || title.Length < 2)
                {
                    continue;
                }

                var match = Regex.Match(href, @"-(?:(\d{4})-)?(\d+)$");
                string id;
                int? parsedYear = null;
                if (match.Success)
                {
                    if (match.Groups[1].Success && int.TryParse(match.Groups[1].Value, out var y))
                    {
                        parsedYear = y;
                    }
                    id = match.Groups[2].Value;
                }
                else
                {
                    id = href;
                }

                if (!seenIds.Add(id))
                {
                    continue;
                }

                results.Add(new FilmwebSearchResult
                {
                    Id = id,
                    Title = title,
                    Year = parsedYear ?? year,
                    Url = href.StartsWith("http") ? href : $"{BaseUrl}{href}",
                    ItemType = href.Contains("/serial/") ? "serial" : "film"
                });
            }
        }

        return results;
    }

    public async Task<FilmwebDetails?> GetDetailsAsync(string urlOrId, string itemType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(urlOrId))
        {
            return null;
        }

        var targetUrl = urlOrId.StartsWith("http") 
            ? urlOrId 
            : $"{BaseUrl}/{(itemType == "serial" ? "serial" : "film")}/-{urlOrId}";

        string html;
        try
        {
            html = await _httpClient.GetStringAsync(targetUrl, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            return null;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var jsonLdNode = doc.DocumentNode.SelectSingleNode("//script[@type='application/ld+json']");
        if (jsonLdNode == null)
        {
            return null;
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonLdNode.InnerText);
            var root = jsonDoc.RootElement;

            var details = new FilmwebDetails
            {
                Id = urlOrId,
                Title = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
                Overview = root.TryGetProperty("description", out var descProp) ? descProp.GetString() : null,
                ItemType = itemType
            };

            if (root.TryGetProperty("image", out var imgProp))
            {
                details.PosterUrl = imgProp.GetString();
            }

            if (root.TryGetProperty("dateCreated", out var dateProp) && DateTime.TryParse(dateProp.GetString(), out var date))
            {
                details.Year = date.Year;
            }
            else if (root.TryGetProperty("startDate", out var sDateProp) && DateTime.TryParse(sDateProp.GetString(), out var sDate))
            {
                details.Year = sDate.Year;
            }

            if (root.TryGetProperty("aggregateRating", out var ratingObj) && ratingObj.TryGetProperty("ratingValue", out var rVal))
            {
                if (float.TryParse(rVal.GetString(), out var rate))
                {
                    details.Rating = rate;
                }
            }

            if (root.TryGetProperty("genre", out var genreProp))
            {
                if (genreProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var g in genreProp.EnumerateArray())
                    {
                        if (g.GetString() is { } genreStr && !string.IsNullOrWhiteSpace(genreStr))
                        {
                            details.Genres.Add(genreStr);
                        }
                    }
                }
                else if (genreProp.GetString() is { } genreStr && !string.IsNullOrWhiteSpace(genreStr))
                {
                    details.Genres.Add(genreStr);
                }
            }

            if (root.TryGetProperty("actor", out var actorProp) && actorProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var actor in actorProp.EnumerateArray())
                {
                    if (actor.TryGetProperty("name", out var actName) && actName.GetString() is { } aName)
                    {
                        details.Actors.Add(aName);
                    }
                }
            }

            if (root.TryGetProperty("director", out var dirProp) && dirProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var dir in dirProp.EnumerateArray())
                {
                    if (dir.TryGetProperty("name", out var dirName) && dirName.GetString() is { } dName)
                    {
                        details.Directors.Add(dName);
                    }
                }
            }

            return details;
        }
        catch
        {
            return null;
        }
    }
}
