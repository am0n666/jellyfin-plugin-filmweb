using System.Collections.Generic;

namespace Jellyfin.Plugin.Filmweb.Api;

public class FilmwebSearchResult
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public int? Year { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
    public string ItemType { get; set; } = "film";
}

public class FilmwebDetails
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? OriginalTitle { get; set; }
    public string? Overview { get; set; }
    public int? Year { get; set; }
    public float? Rating { get; set; }
    public int? VoteCount { get; set; }
    public string? PosterUrl { get; set; }
    public List<string> Genres { get; set; } = [];
    public List<string> Directors { get; set; } = [];
    public List<string> Actors { get; set; } = [];
    public List<string> ProductionCountries { get; set; } = [];
    public string ItemType { get; set; } = "film";
}
