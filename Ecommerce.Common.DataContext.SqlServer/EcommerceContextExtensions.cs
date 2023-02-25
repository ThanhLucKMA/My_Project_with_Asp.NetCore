using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SolidEdu.Share;

public static class EcommerceContextExtensions
{
    /// <summary>
 	/// Adds EcommerceContext to the specified IServiceCollection. Uses the SqlServer database provider.
 	/// </summary>
 	/// <param name="services"></param>
 	/// <param name="connectionString">Set to override the default.</param>
 	/// <returns>An IServiceCollection that can be used to add more services.</returns>
    public static IServiceCollection AddEcommerceContext(this IServiceCollection services, string connectionString = "Data Source=.;Initial Catalog=SolidStore;"+"Integrated Security=true;MultipleActiveResultsets=true;")
    {
        services.AddDbContext<SolidStoreContext>(options => options.UseSqlServer(connectionString));
        return services;
    }
}