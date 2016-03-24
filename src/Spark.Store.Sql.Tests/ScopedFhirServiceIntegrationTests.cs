﻿using System;
using System.Net;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spark.Engine.Core;
using Spark.Engine.Service;
using Spark.Service;

namespace Spark.Store.Sql.Tests
{
    [TestClass]
    public class ScopedFhirServiceIntegrationTests
    {
        private IScopedFhirService<Project> serviceProject;
        private Project project1;
        private Project project2;
        [TestInitialize]
        public void TestInitialize()
        {
            Uri uri = new Uri("http://localhost:49911/fhir", UriKind.Absolute);
            GenericScopedFhirServiceFactory factory = new SqlScopedFhirServiceFactory();
            serviceProject = factory.GetFhirService<Project>(uri, p => p.ScopeKey);

            project1 = new Project() {ScopeKey = 1};
            project2 = new Project() {ScopeKey = 2};

        }

        [TestMethod]
        public void ScopedFhirService_AddResource_GetResourceReturnsSameResource()
        {
            Key patientKey = new Key(String.Empty, "Patient", null, null);
            FhirResponse response = serviceProject.WithScope(project1).Create(patientKey, GetNewPatient(patientKey));
            response = serviceProject.WithScope(project1).Read(response.Resource.ExtractKey().WithoutVersion());
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            response = serviceProject.WithScope(project2).Read(response.Resource.ExtractKey().WithoutVersion());
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            response = serviceProject.WithScope(project1).Search("Patient", new SearchParams());
            Assert.AreEqual(1, ((Bundle)response.Resource).TotalElement.Value);

            response = serviceProject.WithScope(project2).Search("Patient", new SearchParams());
            Assert.AreEqual(0, ((Bundle)response.Resource).TotalElement.Value);
        }


        private static Patient GetNewPatient(Key key)
        {
            Patient selena = new Patient();

            var name = new HumanName();
            name.GivenElement.Add(new FhirString("Selena"));
            name.FamilyElement.Add(new FhirString("Gomez"));
            selena.Name.Add(name);

            var address = new Address();
            address.LineElement.Add(new FhirString("Cornett"));
            address.CityElement = new FhirString("Amanda");
            address.CountryElement = new FhirString("United States");
            address.StateElement = new FhirString("Texas");
            selena.Address.Add(address);

            var contact = new Patient.ContactComponent();
            var contactname = new HumanName();
            contactname.GivenElement.Add(new FhirString("Martijn"));
            contactname.FamilyElement.Add(new FhirString("Harthoorn"));
            contact.Name = contactname;
            selena.Contact.Add(contact);

            selena.Gender = AdministrativeGender.Female;
            selena.Id = key.ToString();
            return selena;
        }
    }


}