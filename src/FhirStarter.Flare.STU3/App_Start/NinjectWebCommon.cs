using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using FhirStarter.Bonfire.STU3.Interface;
using FhirStarter.Bonfire.STU3.Log;
using FhirStarter.Bonfire.STU3.Validation;
using FhirStarter.Flare.STU3;
using Hl7.Fhir.Model;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Syntax;
using Ninject.Web.Common;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace FhirStarter.Flare.STU3
{
    public static class NinjectWebCommon
    {
        private static readonly log4net.ILog Log =
            log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static int _amountOfInitializedIFhirServices;
        private static int _amountOfIFhirStructureDefinitionsInitialized;

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
                var fhirStructureDefinition = typeof(IFhirStructureDefinitionService);

                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var classType in asm.GetTypes())
                    {
                        BindIFhirServices(kernel, fhirService, fhirStructureDefinition, classType);
                    }

                }
                BindProfileValidator(kernel);
            }
            catch (ReflectionTypeLoadException ex)
            {
                ExceptionLogger.LogReflectionTypeLoadException(ex);
            }

            CheckForLackingServices();
           
        }

        private static void CheckForLackingServices()
        {
            var strBuilder = new StringBuilder();
            if (_amountOfInitializedIFhirServices == 0)
            {
                const string iFhirServiceErrorMessage = "No services using " + nameof(IFhirService) +
                                                        " interface has been found and initalized, therefore the FHIR service will not work. Also causes Ninject to throw an exception with \"Server error\" message. Please implement a FHIR service using the " +
                                                        nameof(IFhirService) + " interface";
                strBuilder.AppendLine(iFhirServiceErrorMessage);
                Log.Fatal(iFhirServiceErrorMessage);
            }
            if (_amountOfIFhirStructureDefinitionsInitialized == 0)
            {
                const string structureDefinitionErrorMessage = "Class(es) using " + nameof(IFhirStructureDefinitionService) +
                                                               " was not found in any of the dlls used by the web service. In order for " +
                                                               nameof(StructureDefinition) +
                                                               "s to be availble, please implement a class using the interface which defines where the " +
                                                               nameof(StructureDefinition) + "s can be found.";
                strBuilder.AppendLine(structureDefinitionErrorMessage);
                Log.Warn(structureDefinitionErrorMessage);
            }
            if (strBuilder.Length > 0)
            {
                throw new ArgumentException(strBuilder.ToString());
            }
        }

        private static void BindProfileValidator(IBindingRoot kernel)
        {
            var setting = ConfigurationManager.AppSettings["EnableValidation"];

            var location = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var directoryInfo = new FileInfo(location.AbsolutePath).Directory;
            if (setting == null || !Convert.ToBoolean(setting)) return;
            if (directoryInfo != null)
            {
                var instance = new ProfileValidator(true, true, false,
                    directoryInfo.FullName + @"\Resources\StructureDefinitions");
                kernel.Bind<ProfileValidator>().ToConstant(instance);
            }
        }

        private static void BindIFhirServices(IBindingRoot kernel, Type fhirService, Type fhidStructureDefinition,
            Type classType)
        {
            if (!fhirService.IsAssignableFrom(classType) && !fhidStructureDefinition.IsAssignableFrom(classType) ||
                classType.IsInterface || classType.IsAbstract) return;
            if (fhirService.IsAssignableFrom(classType))
            {
                var instance = (IFhirService) Activator.CreateInstance(classType);
                kernel.Bind<IFhirService>().ToConstant(instance);
                _amountOfInitializedIFhirServices++;
            }
            else
            {
                var instance = (IFhirStructureDefinitionService) Activator.CreateInstance(classType);
                kernel.Bind<IFhirStructureDefinitionService>().ToConstant(instance);
                _amountOfIFhirStructureDefinitionsInitialized++;
            }
        }
    }
}
