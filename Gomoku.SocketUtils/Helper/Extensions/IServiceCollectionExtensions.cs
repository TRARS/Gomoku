using Gomoku.SocketUtils.Helper.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gomoku.SocketUtils.Helper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddAllService(this IServiceCollection services)
        {
            // 注册Json服务
            services.TryAddSingleton<IJsonService, JsonService>();
            // 注册Deflate服务
            services.TryAddSingleton<IDeflateService, DeflateService>();
            return services;
        }
    }
}
