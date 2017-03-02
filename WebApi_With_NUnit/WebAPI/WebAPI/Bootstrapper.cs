using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc4;
using System.Web.Http;
using BusinessServices;
//using DataModel.UnitOfWork;
using Resolver;
using DataModel.UnitOfWork;

namespace WebAPI
{
  public static class Bootstrapper
  {
      public static void Initialise()
      {
          var container = BuildUnityContainer();

          System.Web.Mvc.DependencyResolver.SetResolver(new UnityDependencyResolver(container));
          GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
      }

      private static IUnityContainer BuildUnityContainer()
      {
          var container = new UnityContainer();

          // register all your components with the container here
          // it is NOT necessary to register your controllers

          // e.g. container.RegisterType<ITestService, TestService>();  
          //container.RegisterType<IProductServices, ProductServices>().RegisterType<UnitOfWork>(new HierarchicalLifetimeManager());

          RegisterTypes(container);

          return container;
      }

      public static void RegisterTypes(IUnityContainer container)
      {
            // Component initialization via MEF
            ComponentLoader.LoadContainer(container, ".\\bin", "WebAPI.dll");
            ComponentLoader.LoadContainer(container, ".\\bin", "BusinessServices.dll");
      }
   }
}