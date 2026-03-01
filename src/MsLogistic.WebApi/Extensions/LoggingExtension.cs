using Microsoft.Extensions.Hosting;
using Serilog;

namespace MsLogistic.WebApi.Extensions;

public static class LoggingExtension {
    public static IHostBuilder AddLogging(this IHostBuilder builder) {
        return builder.UseSerilog((ctx, config) => {
            config
                .ReadFrom.Configuration(ctx.Configuration)
                .WriteTo.Console()
                .WriteTo.Seq(ctx.Configuration["Seq:Url"] ?? "http://localhost:5341");
        });
    }
}
