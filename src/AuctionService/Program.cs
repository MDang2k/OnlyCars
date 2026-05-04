using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg => cfg.LicenseKey = "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxODA5MjE2MDAwIiwiaWF0IjoiMTc3Nzc2MzE4NCIsImFjY291bnRfaWQiOiIwMTlkZWFlZTcxODY3Y2Q4YTUzZmFjYjZlZjY4NmFhNiIsImN1c3RvbWVyX2lkIjoiY3RtXzAxa3FuZjJ2cWY3MnhtdnBicjNjOTV5cHQ5Iiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.UZ7K-S68lE6n6-giLFc-LRISAUBzknMYr16Py9w2nm5-q_CwH1IKJJA1P-tNn-EItY-9EO35Kr-vmqKMthb1KvhvWgCgFx1t3UYl50MVyO_z4nWZmRSaeCp0-RpvCmiv_TbHlCXltxF1AAuGQKzilfAUjMVTOsYkfU73IKKBZD_wDPY9BIIksbNlRO92pkVaXfv_3KDeLyXrTSjoFxkjzT_Xu67lHhwnVGUtuF_7hY3JMqxmVqR_Mc-njCjoFWOTP_7KenMV1zhKh9PEo_yM18PvsG5cblbdPogvbowoZmkJTcZOr9GooRnNAcr8Mc63MAb51KC3tKs3k8jLchsBWQ", AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitilizer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
}

app.Run();

