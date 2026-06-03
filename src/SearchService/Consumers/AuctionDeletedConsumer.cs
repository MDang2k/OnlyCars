using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"Received AuctionDeleted event for AuctionId: {context.Message.Id}");

        var db = await DB.InitAsync("SearchDb", null);

        var result = await db.DeleteAsync<Item>(context.Message.Id);

        if (!result.IsAcknowledged) throw new MessageException(typeof(AuctionDeleted), $"Failed to delete item with ID: {context.Message.Id}");
    }
}