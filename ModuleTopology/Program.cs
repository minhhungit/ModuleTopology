using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ModuleTopology
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var moduleLoader = new ModuleLoader();
            var sortedModules = moduleLoader.LoadModules();

            foreach (var moduleType in sortedModules)
            {
                Console.WriteLine(moduleType.Name);
            }

            Console.WriteLine("Hello, World!");
        }
    }

    public abstract class AppModule
    {
        public virtual void ConfigureServices(ServiceConfigurationContext ctx)
        {
            // Implement your module-specific service configuration here
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DependsOnAttribute : Attribute
    {
        public Type[] DependedModuleTypes { get; }

        public DependsOnAttribute(params Type[] dependedModuleTypes)
        {
            DependedModuleTypes = dependedModuleTypes;
        }
    }

    public class ModuleLoader
    {
        public List<Type> LoadModules()
        {
            var moduleTypes = new List<Type>();

            var applicationPath = AppDomain.CurrentDomain.BaseDirectory;
            var moduleFiles = Directory.GetFiles(applicationPath, "*.dll", SearchOption.TopDirectoryOnly);

            foreach (var moduleFile in moduleFiles)
            {
                var assembly = Assembly.LoadFrom(moduleFile);
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && t.BaseType == typeof(AppModule) && t.GetCustomAttribute<DependsOnAttribute>() != null);

                moduleTypes.AddRange(types);
            }

            var dependencyResolver = new ModuleDependencyResolver();
            var sortedModules = dependencyResolver.ResolveModuleDependencies(moduleTypes);

            return sortedModules;
        }
    }

    public class ModuleDependencyResolver
    {
        public List<Type> ResolveModuleDependencies(List<Type> moduleTypes)
        {
            var visited = new HashSet<Type>();
            var sortedModules = new List<Type>();

            foreach (var moduleType in moduleTypes)
            {
                Visit(moduleType, visited, sortedModules);
            }

            //sortedModules.Reverse();
            return sortedModules;
        }

        private void Visit(Type moduleType, HashSet<Type> visited, List<Type> sortedModules)
        {
            if (!visited.Contains(moduleType))
            {
                visited.Add(moduleType);

                var dependsOnAttributes = moduleType.GetCustomAttributes(typeof(DependsOnAttribute), inherit: true) as DependsOnAttribute[];

                if (dependsOnAttributes != null)
                {
                    foreach (var dependsOnAttribute in dependsOnAttributes)
                    {
                        foreach (var dependencyType in dependsOnAttribute.DependedModuleTypes)
                        {
                            Visit(dependencyType, visited, sortedModules);
                        }
                    }
                }

                sortedModules.Add(moduleType);
            }
        }
    }

    // Module 1
    [DependsOn(typeof(Module2))]
    public class Module1: AppModule
    {
        public override void ConfigureServices(ServiceConfigurationContext ctx)
        {
            // Module 1 specific service registrations
        }
    }

    // Module 2
    [DependsOn(typeof(Module3), typeof(Module4))]
    public class Module2: AppModule
    {
        public override void ConfigureServices(ServiceConfigurationContext ctx)
        {
            // Module 1 specific service registrations
        }
    }

    // Module 3
    public class Module3: AppModule
    {
        public override void ConfigureServices(ServiceConfigurationContext ctx)
        {
            // Module 1 specific service registrations
        }
    }

    // Module 4
    [DependsOn(typeof(Module3))]
    public class Module4: AppModule
    {
        public override void ConfigureServices(ServiceConfigurationContext ctx)
        {
            //var services = ctx.Services;

            //// Registering module-specific services
            //services.AddTransient<IMyService, MyService>();
            //services.AddScoped<IMyRepository, MyRepository>();

            //// Configuring existing services
            //services.Configure<SomeOptions>(options =>
            //{
            //    options.SomeSetting = "Value";
            //});
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Application-specific service registrations

            // loads and configures the modules' services automatically
            var moduleLoader = new ModuleLoader();
            var moduleTypes = moduleLoader.LoadModules();
            foreach (var moduleType in moduleTypes)
            {
                var module = Activator.CreateInstance(moduleType) as AppModule;
                if (module != null)
                {
                    module.ConfigureServices(new ServiceConfigurationContext(services));
                }                
            }

            // Additional application-specific service registrations
        }

        public void Configure(IApplicationBuilder app)
        {
            // Application-specific middleware pipeline configurations

            // We does not load or configure the modules' pipelines automatically
            // You can manually configure module-specific middleware here
            app.UseModule1Middleware();
            app.UseModule2Middleware();

            // Additional application-specific middleware pipeline configurations
        }
    }

    public static class ModuleAMiddlewareExtensions
    {
        public static IApplicationBuilder UseModule1Middleware(this IApplicationBuilder app)
        {
            // Configure module-specific middleware here
            app.UseMiddleware<ModuleAMiddleware>();

            return app;
        }
    }

    public class ModuleAMiddleware
    {
        private readonly RequestDelegate _next;

        public ModuleAMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Custom module-specific middleware logic goes here

            await _next(context);
        }
    }

    public static class ModuleBMiddlewareExtensions
    {
        public static IApplicationBuilder UseModule2Middleware(this IApplicationBuilder app)
        {
            // Configure module-specific middleware here
            app.UseMiddleware<ModuleBMiddleware>();

            return app;
        }
    }

    public class ModuleBMiddleware
    {
        private readonly RequestDelegate _next;

        public ModuleBMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Custom module-specific middleware logic goes here

            await _next(context);
        }
    }

    public class ServiceConfigurationContext
    {
        public IServiceCollection Services { get; }

        public ServiceConfigurationContext(IServiceCollection services)
        {
            Services = services;
        }
    }
}


