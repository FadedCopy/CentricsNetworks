using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Centrics.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.MySql;
using Hangfire.MySql.Core;
using Hangfire.Storage;
using jsreport.AspNetCore;
using jsreport.Binary;
using jsreport.Local;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Centrics
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
                           
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            services.Add(new ServiceDescriptor(typeof(CentricsContext), new CentricsContext(Configuration.GetConnectionString("DefaultConnection"))));
            // Adds a default in-memory implementation of IDistributedCache.
            services.AddDistributedMemoryCache();
            services.AddHangfire(configuration => Configuration.GetConnectionString("DefaultConnection"));
            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

            GlobalConfiguration.Configuration.UseStorage(
    new MySqlStorage(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
            });
            services.AddJsReport(new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create());
            services.AddTransient<IEmailService, EmailService>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env,IRecurringJobManager recurringJobs)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHangfireServer();

            //remove the dashboard before production
            app.UseHangfireDashboard();
            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });


            //To clear scheduler on startup.
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringjob in connection.GetRecurringJobs())
                {
                    recurringJobs.RemoveIfExists(recurringjob.Id);
                }
            }


            //actual uncomment it later
            //recurringJobs.AddOrUpdate("EmailStartup", Job.FromExpression<CentricsContext>(x => x.Emailcaller()), Cron.Daily());
            
            //testing purpose print 878smh every 2 mins
            recurringJobs.AddOrUpdate("EmailStartup",Job.FromExpression<CentricsContext>(x => x.callmemaybe()),Cron.MinuteInterval(2));



        }
    }
}
