    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using NHibernate;



    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.UseUrls("http://0.0.0.0:5000");

    builder.Services.AddSingleton<ISessionFactory>(NHibernateHelper.SessionFactory);

    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    builder.Services.AddControllers();

    var app = builder.Build();

    app.MapControllers();

    app.MapGet("/", () => "API-a Martxan dago");

    app.Run();