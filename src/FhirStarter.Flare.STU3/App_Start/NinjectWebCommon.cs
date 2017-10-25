using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using FhirStarter.Bonfire.STU3.Interface;
using FhirStarter.Bonfire.STU3.Log;
using FhirStarter.Bonfire.STU3.Service;
using FhirStarter.Bonfire.STU3.Validation;
using FhirStarter.Flare.STU3;
using FhirStarter.Flare.STU3.Helper;
using FhirStarter.Flare.STU3.Initializers;
using Hl7.Fhir.Model;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Syntax;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;

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
        private static int _amountOfInitializedIFhirMockupServices;

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
                var mockupService = typeof(IFhirMockupService);
                var fhirService = typeof(IFhirService);
                var fhirStructureDefinition = typeof(IFhirStructureDefinitionService);
                //var typesToEnable = new List<TypeInitializer> {mockupService, fhirService, fhirStructureDefinition};
                var serviceTypes = new List<TypeInitializer>
                {
                    new TypeInitializer(false, mockupService, nameof(IFhirMockupService)),
                    new TypeInitializer(true, fhirService, nameof(IFhirService)),
                    new TypeInitializer(true, fhirStructureDefinition, nameof(IFhirStructureDefinitionService))
                };

                var serviceAssemblies = AssemblyLoaderHelper.GetFhirServiceAssemblies();
                foreach (var asm in serviceAssemblies)
                {
                    foreach (var classType in asm.GetTypes())
                    {
                        BindIFhirServices(kernel, serviceTypes, classType);
                    }
                }

            }
            catch (ReflectionTypeLoadException ex)
            {
                ExceptionLogger.LogReflectionTypeLoadException(ex);
            }

            CheckForLackingServices();

        }

        private static void BindIFhirServices(IBindingRoot kernel, List<TypeInitializer> serviceTypes, Type classType)
        {
            var serviceType = FindType(serviceTypes, classType);
            if (serviceType != null)
            {
                if (serviceType.Name.Equals(nameof(IFhirService)))
                {
                    var instance = (IFhirService)Activator.CreateInstance(classType);
                    kernel.Bind<IFhirService>().ToConstant(instance);
                    _amountOfInitializedIFhirServices++;
                }
                else if (serviceType.Name.Equals(nameof(IFhirMockupService)))
                {
                    var instance = (IFhirMockupService)Activator.CreateInstance(classType);
                    kernel.Bind<IFhirMockupService>().ToConstant(instance);
                    _amountOfInitializedIFhirMockupServices++;
                }
                else if (serviceType.Name.Equals(nameof(IFhirStructureDefinitionService)))
                {
                    var structureDefinitionService = (IFhirStructureDefinitionService)Activator.CreateInstance(classType);
                    kernel.Bind<IFhirStructureDefinitionService>().ToConstant(structureDefinitionService);
                    var validator = structureDefinitionService.GetValidator();
                    if (validator != null)
                    {
                        var profileValidator = new ProfileValidator(validator);
                        kernel.Bind<ProfileValidator>().ToConstant(profileValidator);
                    }
                    _amountOfIFhirStructureDefinitionsInitialized++;
                }
            }
        }

        private static TypeInitializer FindType(List<TypeInitializer> serviceTypes, Type classType)
        {
            foreach (var service in serviceTypes)
            {
                if (service.ServiceType.IsAssignableFrom(classType) && !classType.IsInterface && !classType.IsAbstract)
                    return service;
            }
            return null;
        }

        private static void CheckForLackingServices()
        {
            var throwException = false;
            var strBuilder = new StringBuilder();
            if (_amountOfInitializedIFhirServices == 0)
            {
                const string iFhirServiceErrorMessage = "No services using " + nameof(IFhirService) +
                                                        " interface has been found and initalized, therefore the FHIR service will not work. Also causes Ninject to throw an exception with \"Server error\" message. Please implement a FHIR service using the " +
                                                        nameof(IFhirService) + " interface";
                strBuilder.AppendLine(iFhirServiceErrorMessage);
                Log.Fatal(iFhirServiceErrorMessage);
                throwException = true;
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
            if (_amountOfIFhirStructureDefinitionsInitialized != 1)
            {
                const string structureDefinitionErrorMessage = "Class(es) using " + nameof(IFhirStructureDefinitionService) +
                                                               " was found more than once. In order for " +
                                                               nameof(StructureDefinition) +
                                                               "s to be available, please implement only one class using the interface which defines where the " +
                                                               nameof(StructureDefinition) + "s can be found.";
                strBuilder.AppendLine(structureDefinitionErrorMessage);
                Log.Warn(structureDefinitionErrorMessage);
            }
            if (_amountOfInitializedIFhirMockupServices == 0 && ServiceHandler.IsMockupEnabled())
            {
                string mockupServiceError = "Class(es) using " + nameof(IFhirMockupService) +
                                                   " was not found despite Mockup being enabled through the appSettings key " +
                                                   ServiceHandler.MockupEnabled;

                strBuilder.AppendLine(mockupServiceError);
                Log.Error(mockupServiceError);
            }

            if (strBuilder.Length > 0 && throwException)
            {
                throw new ArgumentException(strBuilder.ToString());
            }
        }


        private static void BindIFhirServices(IBindingRoot kernel, Type fhirService, Type fhidStructureDefinition,
            Type classType)
        {
            if (!fhirService.IsAssignableFrom(classType) && !fhidStructureDefinition.IsAssignableFrom(classType) ||
                classType.IsInterface || classType.IsAbstract) return;
            if (fhirService.IsAssignableFrom(classType))
            {
                var instance = (IFhirService)Activator.CreateInstance(classType);
                kernel.Bind<IFhirService>().ToConstant(instance);
                _amountOfInitializedIFhirServices++;
            }
            else
            {
                var structureDefinitionService = (IFhirStructureDefinitionService)Activator.CreateInstance(classType);
                kernel.Bind<IFhirStructureDefinitionService>().ToConstant(structureDefinitionService);
                var validator = structureDefinitionService.GetValidator();
                if (validator != null)
                {
                    var profileValidator = new ProfileValidator(validator);
                    kernel.Bind<ProfileValidator>().ToConstant(profileValidator);
                }
                _amountOfIFhirStructureDefinitionsInitialized++;
            }
        }
    }
}