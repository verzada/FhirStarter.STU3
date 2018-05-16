using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using FhirStarter.Inferno.IntegrationTests.Helpers;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using NUnit.Framework;

namespace FhirStarter.Inferno.IntegrationTests.Client
{
    [TestFixture]
    internal class CommunicationRequestClientTest
    {

        private ICollection<FhirClient> FhirClients { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            FhirClients = FhirClientHelper.GetFhirClients();
        }

        [TestCase("SampleCommunicationRequest.xml", 100, 100)]
        public void CreateCommunicationRequestStressTest(string path, int numberOfTries, int numberOfParallellCalls)
        {
            CommunicationRequest request;
            using (var stream = AssemblyHelper.GetStream(path, Assembly.GetExecutingAssembly()))
            {
                var xDoc = XDocument.Load(stream);
                request = new FhirXmlParser().Parse<CommunicationRequest>(xDoc.ToString());
            }

            var requests = new List<CommunicationRequest>();
            for (var i = 0; i < numberOfTries; i++)
            {
                requests.Add(request);
            }

            long total = 0;
            foreach (var client in FhirClients)
            {
                //Parallel.ForEach(requests, new ParallelOptions { MaxDegreeOfParallelism = numberOfParallellCalls }, communicationRequest =>
                Parallel.ForEach(requests, communicationRequest =>
                {
                    var watch = new Stopwatch();
                    watch.Start();

                    var result = client.Create(communicationRequest);
                    watch.Stop();
                    Console.WriteLine("Elapsed: " + watch.ElapsedMilliseconds);
                    total += watch.ElapsedMilliseconds;
                });
            }
            Console.WriteLine("Total: " + total);
        }
    }
}
