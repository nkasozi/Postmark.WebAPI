using Xunit;
using Postmark.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Postmark.Interview;
using System.Threading.Tasks;
using Newtonsoft.Json;
using FluentAssertions;
using System.Net;
using Postmark.WebAPI.Models;
using System.Net.Http;

namespace Postmark.WebAPI.Controllers.Tests
{
    public class EmailControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly string WEBAPI_ROOT_URL = "/api/email";

        public EmailControllerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Given_ValidUserSubscriptionRequest_Returns_Success()
        {
            // Arrange
            var url = $"{WEBAPI_ROOT_URL}";
            var client = _factory.CreateClient();
            var ID = "1234";
            var details = new SingleEmail
            {
                To = "test22@test.com",
                HtmlBody = "Test",
                UniqueEmailID = ID
            };

            var jsonReq = JsonConvert.SerializeObject(details);
            var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");


            // Act
            var response = await client.PostAsync(url,content);
            var json = await response.Content.ReadAsStringAsync();
            var answer = JsonConvert.DeserializeObject<SingleEmailResult>(json);


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            answer.Should().NotBeNull();
            answer.ErrorCode.Should().Be(OpResultCode.SUCCCESS);
            answer.Message.Should().NotBeNullOrEmpty();
            answer.To.Should().NotBeNullOrEmpty();
            answer.MessageID.Should().NotBeNullOrEmpty();
            answer.UniqueEmailID.Should().Be(ID);

            response.Content.Headers.ContentType.Should().NotBeNull();
        }

        //test bulk valid email
        [Fact()]
        public async Task Given_ValidBulkEmail_ExpectSuccess()
        {
            //arrange
            var ID = "1234";
            var ID2 = "12345";
            var ID3 = "12346";
            var testEmail = "test333@test.com";

            var details = new List<SingleEmail>();

            details.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test",
                    UniqueEmailID = ID
                }
            );

            details.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test 2",
                    UniqueEmailID = ID2
                }
             );

            details.Add
            (
                new SingleEmail
                {
                    To = testEmail,
                    HtmlBody = "Test 3",
                    UniqueEmailID = ID3
                }
            );

            // Arrange
            var url = $"{WEBAPI_ROOT_URL}/batch";
            var client = _factory.CreateClient();

            var jsonReq = JsonConvert.SerializeObject(details);
            var content = new StringContent(jsonReq, Encoding.UTF8, "application/json");


            // Act
            var response = await client.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();
            var answer = JsonConvert.DeserializeObject<List<SingleEmailResult>>(json);


            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);


            //assert
            answer.Should().NotBeNull();
            answer.Count.Should().Be(details.Count);

            //check that each property is filled
            foreach (var emailResult in answer)
            {
                emailResult.Should().NotBeNull();
                emailResult.ErrorCode.Should().Be(OpResultCode.SUCCCESS);
                emailResult.Message.Should().NotBeNullOrEmpty();
                emailResult.To.Should().NotBeNullOrEmpty();
                emailResult.MessageID.Should().NotBeNullOrEmpty();
                emailResult.UniqueEmailID.Should().NotBeNullOrEmpty();
                Assert.Contains(details, x => x.UniqueEmailID == emailResult.UniqueEmailID);
            }

        }

    }
}