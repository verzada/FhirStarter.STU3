using System;
using System.Reflection;
using System.Web;
using FhirStarter.Bonfire.STU3.Interface;
using FhirStarter.Bonfire.STU3.Log;
using FhirStarter.Bonfire.STU3.Validation;
using FhirStarter.Flare.STU3;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace FhirStarter.Flare.STU3
{
    public static class NinjectWebCommon
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Bootstrapper _bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            _bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            _bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {

            try
            {
                var fhirService = typeof(IFhirService);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type classType in asm.GetTypes())
                    {
                        BindIFhirServices(kernel, fhirService, classType);
                    }
                }
                BindProfileValidator(kernel);
            }
            catch (ReflectionTypeLoadException ex)
            {
                ExceptionLogger.LogReflectionTypeLoadException(ex);
            }

        }

        private static void BindProfileValidator(IKernel kernel)
        {
            var instance = new ProfileValidator(true, true, false, Assembly.GetExecutingAssembly());
            kernel.Bind<ProfileValidator>().ToConstant(instance);
        }

        private static void BindIFhirServices(IKernel kernel, Type fhirService, Type classType)
        {
            if (fhirService.IsAssignableFrom(classType) && !classType.IsInterface && !classType.IsAbstract)
            {
                var instance = (IFhirService) Activator.CreateInstance(classType);
                kernel.Bind<IFhirService>().ToConstant(instance);
            }
        }
    }
}
