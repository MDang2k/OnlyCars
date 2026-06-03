using Contracts;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODA5MjE2MDAwIiwiaWF0IjoiMTc3Nzc2MzE4NCIsImFjY291bnRfaWQiOiIwMTlkZWFlZTcxODY3Y2Q4YTUzZmFjYjZlZjY4NmFhNiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa3FuZjJ2cWY3MnhtdnBicjNjOTV5cHQ5Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.UZ7K-S68lE6n6-giLFc-LRISAUBzknMYr16Py9w2nm5-q_CwH1IKJJA1P-tNn-EItY-9EO35Kr-vmqKMthb1KvhvWgCgFx1t3UYl50MVyO_z4nWZmRSaeCp0-RpvCmiv_TbHlCXltxF1AAuGQKzilfAUjMVTOsYkfU73IKKBZD_wDPY9BIIksbNlRO92pkVaXfv_3KDeLyXrTSjoFxkjzT_Xu67lHhwnVGUtuF_7hY3JMqxmVqR_Mc-njCjoFWOTP_7KenMV1zhKh9PEo_yM18PvsG5cblbdPogvbowoZmkJTcZOr9GooRnNAcr8Mc63MAb51KC3tKs3k8jLchsBWQ", AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, TimeSpan.FromSeconds(5, 5)));

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
    }
});



app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() => HttpPolicyExtensions
.HandleTransientHttpError()
.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
.WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));