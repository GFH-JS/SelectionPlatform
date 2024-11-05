
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using BookStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;
using SelectionPlatform.Auth;
using SelectionPlatform.Configuration;
using SelectionPlatform.Core.Config;
using SelectionPlatform.IRepository;
using SelectionPlatform.IServices;
using SelectionPlatform.Mapping;
using SelectionPlatform.Repository;
using SelectionPlatform.Services;
using SelectionPlatform.Services.CentrifugeCalculate;
using SelectionPlatform.Utility.Helper;
using SelectionPlatformWeb.Extentions;
using System.Configuration;
using System.Text.Unicode;

namespace SelectionPlatformWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //使用autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder => {
                //containBuilder.RegisterType<CentrifugePrjInfoSeleciton>().As<ICentrifugePrjInfoSeleciton>().InstancePerDependency();
                //获取所有控制器类型并使用属性注入
                var controllerBaseType = typeof(ControllerBase);
                containerBuilder.RegisterAssemblyTypes(typeof(Program).Assembly)
                    .Where(t => controllerBaseType.IsAssignableFrom(t) && t != controllerBaseType)
                    .PropertiesAutowired();

                containerBuilder.RegisterModule(new AutofacModuleRegister());
              
                //containerBuilder.RegisterType<JwtServices>().As<IJwtServices>().SingleInstance();
                //containerBuilder.RegisterType<InitData>();  // 测试用  无接口注入
                //containerBuilder.RegisterType<CentrifugeCalculateHelper>().InstancePerDependency();
                //containerBuilder.RegisterType<RepositoryWrapper>().As<IRepositoryWrapper>().InstancePerLifetimeScope();  这样接口+实现 可修改生命周期
            });
            // Add services to the container.
            builder.Services.AddControllers().AddNewtonsoftJson(option => { 
            
                option.SerializerSettings.ContractResolver = new DefaultContractResolver();
                option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                
            });
           
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option => { 
            //添加过滤器 swager请求默认值

            });
            builder.Services.ConfigureCors();
            builder.Services.AddMysqlSetup(builder.Configuration);
            builder.Logging.AddLog4Net("log4net.config");
            builder.Services.AddAutoMapper(typeof(AutoMapperConfiguration));
            builder.Services.AddAuthorizationSetupForClient(builder.Configuration);

           

            #region mongodb
            builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSetting"));
            MongoDbSettings mongoDbSettings = new MongoDbSettings();
            builder.Configuration.Bind("MongoDbSetting", mongoDbSettings);
            builder.Services.AddMongoSetup(mongoDbSettings); 
            #endregion

            //builder.Services.AddSingleton<InitData>();  // 测试用  无接口注入
            //builder.Services.AddTransient<CentrifugeCalculateHelper>();

           //builder.WebHost.UseKestrel(option => {
           //    option.ListenAnyIP(5000);
           //    option.ListenAnyIP(5001, listenOptions =>
           //    {
           //        listenOptions.UseHttps();
           //    });
           //});

           //builder.Services.AddTransient<ICentrifugePrjInfoSeleciton,CentrifugePrjInfoSeleciton>();


           var app = builder.Build();

            // Configure the HTTP request pipeline. 中间件处理 管道
            if (app.Environment.IsDevelopment())
            {
                //app.UseSwagger();
                //app.UseSwaggerUI();
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("corsPolicy");
            app.UseHttpsRedirection();

            app.UseAuthentication();// 启用身份验证
            app.UseAuthorization();// 启用授权
            



            app.MapControllers();

            app.Run();
        }
    }
}
