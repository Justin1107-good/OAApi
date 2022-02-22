using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OASystemSynergy.Log4Net;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using SynergyCommon;
using SynergyCommon.Context;
using SynergyCore;
using SynergyCore.user;
using SynergyEntity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                    builder.WithOrigins("https://localhost:44345/", "https://localhost:5001/", "https://localhost:44308/")//指定域
                                               // builder
                                               // .AllowAnyOrigin() //允许任何来源的主机访问 
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//指定处理cookie 
                });
            });
            #endregion

            #region  //Swagger 配置

            services.AddScoped<SwaggerGenerator>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
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

                //获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                //配置的xml文件名
                var xmlPath = Path.Combine(basePath, "OASystemSynergy.xml");
                //默认的第二个参数是false,对方法的注释
                c.IncludeXmlComments(xmlPath);
                #region 加锁  
                //利用Swagger为我们提供的接口，在AddSwaggerGen服务中，添加保护api资源的描述。
                var openAPISecourtiy = new OpenApiSecurityScheme
                {

                    Description = "JWT认证授权，使用直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",   //jwt 默认参数名称
                    In = ParameterLocation.Header,   //jwt默认存放Authorization信息的位置（请求头）
                    Type = SecuritySchemeType.ApiKey
                };
                c.AddSecurityDefinition("oauth2", openAPISecourtiy);
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                #endregion
            });
            #endregion

            services.AddScoped<SpireDocHelper>();
            var Issurer = "JWTBearer.Auth";  //发行人
            var Audience = "api.auth";       //受众人
            var secretCredentials = "q2xiARx$4x3TKqBJ";   //密钥

            //配置认证服务
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //是否验证发行人
                    ValidateIssuer = true,
                    ValidIssuer = Issurer,//发行人
                    //是否验证受众人
                    ValidateAudience = true,
                    ValidAudience = Audience,//受众人
                    //是否验证密钥
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretCredentials)),

                    ValidateLifetime = true, //验证生命周期
                    RequireExpirationTime = true, //过期时间
                };
            });

            //基于自定义策略授权
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "Cp",
                    policy => policy
                    .Requirements
                    .Add(new PermissionRequirement("admin"))
                    );
            });
            //此外，还需要在 IAuthorizationHandler 类型的范围内向 DI 系统注册新的处理程序：
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
            // 如前所述，要求可包含多个处理程序。如果为授权层的同一要求向 DI 系统注册多个处理程序，有一个成功就足够了。

            services.AddScoped<IUserFactory, UserFactory>();
         




        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, SqlDbContext sqlDbContext, ILoggerFactory loggerFactory)
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
