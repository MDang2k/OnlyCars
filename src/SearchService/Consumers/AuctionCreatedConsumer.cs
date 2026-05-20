using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;

namespace SearchService.Consumers;

public class AuctionCreatedConsumer : IConsumer<AuctionCreated>
{
    private IMapper _mapper;

    public AuctionCreatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionCreated> context)
    {
        Console.WriteLine($"Received AuctionCreated event for AuctionId: {context.Message.Id}");

        var item = _mapper.Map<Item>(context.Message);

        await DB.Default.SaveAsync(item, context.CancellationToken);
    }
}