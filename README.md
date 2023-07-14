# ModuleTopology

```csharp
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
```


### Inspired by abp.io
