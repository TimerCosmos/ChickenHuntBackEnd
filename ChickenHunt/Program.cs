using ChickenHunt.Hubs;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("MyCorsPolicy", policy =>
            {
                policy.AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials()
                      .WithOrigins("http://localhost:4200", "https://chicken-hunt.vercel.app/");
            });
        });

        // Add services to the container.
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
    
        builder.Services.AddControllers();
        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("MyCorsPolicy");
        app.MapHub<GameHub>("/gamehub");
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}