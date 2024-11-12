using Microsoft.Owin;
using Owin;
using Hangfire;
using Microsoft.Owin;


[assembly: OwinStartupAttribute(typeof(CosmeticsStore.Startup))]
namespace CosmeticsStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Đọc chuỗi kết nối từ tệp Web.config
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Cấu hình lưu trữ SQL Server
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);

            // Configure other middleware and pipeline
            // ...

            app.UseHangfireDashboard();
            app.UseHangfireServer();
            ConfigureAuth(app);
        }

    }
}
