using WireMock.Server;
using WireMock.Settings;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using System;

namespace VdrDesktop.Infrastructure
{
    public class MockAuthenticationServer
    {
        private WireMockServer? _server;

        public int Start()
        {
            _server = WireMockServer.Start(new WireMockServerSettings
            {
                Urls = new[] { "http://localhost:0" }, // Port 0 indicates that the OS should select a free port
                StartAdminInterface = true, // Optional: Allows you to manage mocks via a web UI
                ReadStaticMappings = false // Set true if you want to use JSON-based mappings
            });

            ConfigureRoutes();

            Console.WriteLine($"WireMock server running at {_server.Urls[0]}");

            return new Uri(_server.Urls[0]).Port;
        }

        private void ConfigureRoutes()
        {
            _server?.Given(Request.Create()
                    .WithPath("/oauth/token")
                    .UsingPost()
                    .WithBody(body =>
                    {
                        var par = System.Web.HttpUtility.ParseQueryString(body);
                        return par["grant_type"] == "password";
                    }
                ))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(@"
                    {
                        ""access_token"": ""mock_access_token"",
                        ""token_type"": ""Bearer"",
                        ""expires_in"": 3600
                    }"));

            _server?.Given(Request.Create()
                .WithPath("/oauth/token")
                .UsingPost()
                .WithBody(body =>
                    {
                        var par = System.Web.HttpUtility.ParseQueryString(body);
                        return par["grant_type"] == "password" &&
                               par["username"] == "invalid" &&
                               par["password"] == "invalid";
                    }
                ))
                .RespondWith(Response.Create()
                    .WithStatusCode(400)
                    .WithBody(@"{ ""error"": ""invalid_grant"" }"));

            _server.LogEntriesChanged += (sender, args) =>
            {
                foreach (var logEntry in _server.LogEntries)
                {
                    Console.WriteLine($"Request: {logEntry.RequestMessage.Body}");
                }
            };
        }

        public void Stop()
        {
            _server?.Stop();
            _server = null;
        }
    }
}
