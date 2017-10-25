using System;
using FhirStarter.Bonfire.STU3.Parameter;
using Hl7.Fhir.Rest;
using NUnit.Framework;

namespace FhirStarter.UnitTests.Parameter
{
    [TestFixture]
    public class CommonParameterFilterTest
    {

        [Test]
        public void TestSuccessfullSearchParams()
        {
            var searchParams = SetupSearchParams();
            var commonfilter = new CommonParameters(searchParams);

            Assert.IsTrue(commonfilter.HasIdentifier, "Identifier is not found in the commonparameters object");
            Assert.IsTrue(commonfilter.HasName, "Name is not found in the commonparameters object");
            Assert.IsTrue(commonfilter.HasLastUpdatedFrom,
                nameof(commonfilter.HasLastUpdatedFrom) + " is not found in the " + nameof(commonfilter) + " object");
            Assert.IsTrue(commonfilter.HasLastUpdatedTo,
                nameof(commonfilter.HasLastUpdatedTo) + " is not found in the " + nameof(commonfilter) + " object");
        }

        private SearchParams SetupSearchParams()
        {
            var searchParams = new SearchParams();
            searchParams.Add(CommonParameters.ParameterName, "Ola Nordmann");
            searchParams.Add(CommonParameters.ParameterLastUpdated, "2018-01-02");
            searchParams.Add(CommonParameters.ParameterLastUpdated, "2018-02-01");

            searchParams.Add(CommonParameters.ParameterIdentifier, "abcd-efgh-ijkl-mnop-qrst-uvw-xyz");
            return searchParams;
        }

        [Test]
        public void TestFailingSearchParams()
        {
            var searchParams = SetupFailingParams();
            object TestDelegate() => new CommonParameters(searchParams);
            Assert.That(TestDelegate, Throws.TypeOf<ArgumentException>());
        }

        private SearchParams SetupFailingParams()
        {
            var searchParams = new SearchParams();
            searchParams.Add(CommonParameters.ParameterName, "Ola Nordmann");
            searchParams.Add(CommonParameters.ParameterNameContains, "Ola Nordmann");
            return searchParams;
        }
    }
}
