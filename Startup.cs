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
                // ȫ��ע�ᣬȫ����Ч
                options.Filters.Add(typeof(MyExceptionFilterAttribute));
            });
            #region ���ݿ�����
            services.AddDbContext<SqlDbContext>(options =>
            {
                var dataAppSetting = Configuration.GetConnectionString("StrCon");
                if (dataAppSetting == null)
                {
                    throw new Exception("δ�������ݿ�����");
                }
                //server���ӣ�EnableRetryOnFailure��ʾʧ��֧�����ԣ�
                options.UseSqlServer(dataAppSetting, option => option.EnableRetryOnFailure());
            });
            #endregion

            #region //���ÿ�����


            services.AddCors(options =>
            {
                options.AddPolicy("any", builder =>
                {
                    builder.WithOrigins("https://localhost:44345/", "https://localhost:5001/", "https://localhost:44308/")//ָ����
                                               // builder
                                               // .AllowAnyOrigin() //�����κ���Դ���������� 
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();//ָ������cookie 
                });
            });
            #endregion

            #region  //Swagger ����

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

                //��ȡӦ�ó�������Ŀ¼�����ԣ����ܹ���Ŀ¼Ӱ�죬������ô˷�����ȡ·����
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);
                //���õ�xml�ļ���
                var xmlPath = Path.Combine(basePath, "OASystemSynergy.xml");
                //Ĭ�ϵĵڶ���������false,�Է�����ע��
                c.IncludeXmlComments(xmlPath);
                #region ����  
                //����SwaggerΪ�����ṩ�Ľӿڣ���AddSwaggerGen�����У���ӱ���api��Դ��������
                var openAPISecourtiy = new OpenApiSecurityScheme
                {

                    Description = "JWT��֤��Ȩ��ʹ��ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
                    Name = "Authorization",   //jwt Ĭ�ϲ�������
                    In = ParameterLocation.Header,   //jwtĬ�ϴ��Authorization��Ϣ��λ�ã�����ͷ��
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
            var Issurer = "JWTBearer.Auth";  //������
            var Audience = "api.auth";       //������
            var secretCredentials = "q2xiARx$4x3TKqBJ";   //��Կ

            //������֤����
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    //�Ƿ���֤������
                    ValidateIssuer = true,
                    ValidIssuer = Issurer,//������
                    //�Ƿ���֤������
                    ValidateAudience = true,
                    ValidAudience = Audience,//������
                    //�Ƿ���֤��Կ
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretCredentials)),

                    ValidateLifetime = true, //��֤��������
                    RequireExpirationTime = true, //����ʱ��
                };
            });

            //�����Զ��������Ȩ
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    "Cp",
                    policy => policy
                    .Requirements
                    .Add(new PermissionRequirement("admin"))
                    );
            });
            //���⣬����Ҫ�� IAuthorizationHandler ���͵ķ�Χ���� DI ϵͳע���µĴ������
            services.AddScoped<IAuthorizationHandler, PermissionRequirementHandler>();
            // ��ǰ������Ҫ��ɰ����������������Ϊ��Ȩ���ͬһҪ���� DI ϵͳע�������������һ���ɹ����㹻�ˡ�

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
            //����Cors��������MVC֮ǰ
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
