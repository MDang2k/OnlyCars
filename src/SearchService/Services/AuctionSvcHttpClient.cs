using MongoDB.Entities;

namespace SearchService.Services;

public class AuctionSvcHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var db = await DB.InitAsync("SearchDb", null);

        var lastUpdated = db.Find<Item, string>()
            .Sort(x => x.Descending(i => i.UpdatedAt))
            .Project(i => i.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + $"/api/auctions?date={lastUpdated}");
    }
    
}