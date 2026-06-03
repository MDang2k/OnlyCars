using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"Received AuctionUpdated event for AuctionId: {context.Message.Id}");

        var db = await DB.InitAsync("SearchDb", null);

        var item = _mapper.Map<Item>(context.Message);

        var result = await db.Update<Item>()
        .Match(i => i.ID == item.ID)
        .ModifyOnly(x => new
        {
            x.Color,
            x.Make,
            x.Model,
            x.Year,
            x.Mileage,
        }, item)
        .ExecuteAsync();

        if (!result.IsAcknowledged) throw new MessageException(typeof(AuctionUpdated), $"Failed to update item with ID: {item.ID}");
    }
}