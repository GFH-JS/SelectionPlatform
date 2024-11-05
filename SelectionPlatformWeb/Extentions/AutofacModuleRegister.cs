using Amazon.Runtime.Internal.Util;
using Autofac;
using SelectionPlatform.Auth;
using SelectionPlatform.Configuration;
using SelectionPlatform.IRepository;
using SelectionPlatform.IRepository.ProjectInfo;
using SelectionPlatform.IRepository.User;
using SelectionPlatform.Repository;
using SelectionPlatform.Repository.ProjectInfo;
using SelectionPlatform.Services.CentrifugeCalculate;
using SelectionPlatform.Utility.Helper;
using System.Reflection;

namespace SelectionPlatformWeb.Extentions
{
    public class AutofacModuleRegister : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var basePath = AppContext.BaseDirectory;

            #region 带有接口层的服务注入

            var servicesDllFile = Path.Combine(basePath, "SelectionPlatform.Services.dll");
            var repositoryDllFile = Path.Combine(basePath, "SelectionPlatform.Repository.dll");

            if (!(File.Exists(servicesDllFile) && File.Exists(repositoryDllFile)))
            {
                var msg = "Repository.dll和Services.dll 丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。";
                throw new Exception(msg);
            }

            // AOP 开关，如果想要打开指定的功能，只需要在 appsettigns.json 对应对应 true 就行。
            //var cacheType = new List<Type>();
            //if (AppSettingsConstVars.RedisConfigEnabled)
            //{
            //    builder.RegisterType<RedisCacheAop>();
            //    cacheType.Add(typeof(RedisCacheAop));
            //}
            //else
            //{
            //    builder.RegisterType<MemoryCacheAop>();
            //    cacheType.Add(typeof(MemoryCacheAop));
            //}

            // 获取 Service.dll 程序集服务，并注册
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            //支持属性注入依赖重复
            builder.RegisterAssemblyTypes(assemblysServices).AsImplementedInterfaces()  //将类注册位其实现的接口
                .InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            // 获取 Repository.dll 程序集服务，并注册
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            //支持属性注入依赖重复
            var resigtration = builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces().InstancePerDependency()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);


          
            //resigtration.Where(d=>d.Name.Contains("Wrapper")).As<IRepositoryWrapper>().InstancePerLifetimeScope();
            //resigtration.Where(d => d.GetInterfaces().Contains(typeof(IRepositoryWrapper))).InstancePerDependency();

            // 获取 Service.dll 程序集服务，并注册
            //var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            //builder.RegisterAssemblyTypes(assemblysServices)
            //    .AsImplementedInterfaces()
            //    .InstancePerDependency()
            //    .PropertiesAutowired()
            //    .EnableInterfaceInterceptors()//引用Autofac.Extras.DynamicProxy;
            //    .InterceptedBy(cacheType.ToArray());//允许将拦截器服务的列表分配给注册。

            //// 获取 Repository.dll 程序集服务，并注册
            //var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            //builder.RegisterAssemblyTypes(assemblysRepository)
            //    .AsImplementedInterfaces()
            //    .PropertiesAutowired()
            //    .InstancePerDependency();


            #endregion


            
            builder.RegisterType<JwtServices>().As<IJwtServices>().SingleInstance();
            builder.RegisterType<AppSettingsHelper>().WithParameter("contentPath", Environment.CurrentDirectory).SingleInstance();
            builder.RegisterType<InitData>();  // 测试用  无接口注入
            builder.RegisterType<CentrifugeCalculateHelper>().InstancePerDependency();

            ///手动注册 scoped
            builder.RegisterType<RepositoryWrapper>().As<IRepositoryWrapper>().InstancePerLifetimeScope();

           

        }
    }
}
