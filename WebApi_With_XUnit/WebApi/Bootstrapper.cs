using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc4;
using System.Web.Http;
using BusinessServices;
using Resolver;

namespace WebApi
{
  public static class Bootstrapper
  {
    public static void Initialise()
    {
          var container = BuildUnityContainer();

          // register dependency resolver for WebAPI RC
          System.Web.Mvc.DependencyResolver.SetResolver(new UnityDependencyResolver(container));
          GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
    }

    private static IUnityContainer BuildUnityContainer()
    {
        var container = new UnityContainer();

        // register all your components with the container here
        // it is NOT necessary to register your controllers

        // e.g. container.RegisterType<ITestService, TestService>();    

        RegisterTypes(container);

        return container;
    }

    public static void RegisterTypes(IUnityContainer container)
    {
            // Component initialization via MEF
            ComponentLoader.LoadContainer(container, ".\\bin", "WebApi.dll");
            ComponentLoader.LoadContainer(container, ".\\bin", "BusinessServices.dll");
    }
  }
}