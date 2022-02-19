using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OASystemSynergy.Log4Net;
using SynergyCommon.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OASystemSynergy
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

            services.AddControllers();
            services.AddControllersWithViews(options =>
            {
                // 全局注册，全局生效
                options.Filters.Add(typeof(MyExceptionFilterAttribute));
            });
            #region 数据库配置
            services.AddDbContext<SqlDbContext>(options =>
            {
                var dataAppSetting = Configuration.GetConnectionString("StrCon");
                if (dataAppSetting == null)
                {
                    throw new Exception("未配置数据库连接");
                }
                //server连接，EnableRetryOnFailure表示失败支持重试；
                options.UseSqlServer(dataAppSetting, option => option.EnableRetryOnFailure());
            });
            #endregion

            #region //配置跨域处理


            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.WithOrigins("https://localhost:44345/", "https://localhost:5001/")//指定域
                    //builder.AllowAnyOrigin() //允许任何来源的主机访问 
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//指定处理cookie 
                });
            });
            #endregion

            #region  //Swagger 配置


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {
                    Title = "OASystemSynergy",
                    Version = "v1",
                    Description = "OA Synergy System",
                    Contact = new OpenApiContact
                    {
                        Name = "Justin",
                        Email = "wj11074@outlook.com",
                        Url = new Uri("https://bt23285269.icoc.vc/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "licence Justin",
                        Url = new Uri("https://github.com/Justin1107-good")
                    }
                });
            });
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SqlDbContext sqlDbContext,ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "OASystemSynergy v1"));
            }

            app.UseHttpsRedirection();
            
            
            app.UseRouting();
            //配置Cors，必须在MVC之前
            app.UseCors("any");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            sqlDbContext.Database.EnsureCreated();
        }
    }
}
