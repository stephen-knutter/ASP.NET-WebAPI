using System;
using Xunit;
using System.Net.Http;
using Basic_Web_Api.Models;
using Basic_Web_Api.Controllers;

namespace Basic_Web_Api_Tests
{
    [Fact]
    public void TestNewGreetingAdd()
    {
        var greetingName = "newgreeting";
        var greetingMessage = "Hello Test";
        var fakeRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost:63757/api/greeting");
        var greeting = new Greeting
        {
            Name = greetingName,
            Message = greetingMessage
        };
        var service = new GreetingController();
        service.Request = fakeRequest;

        var response = service.PostGreeting(greeting);

        Assert.NotNull(response);
    }
}
