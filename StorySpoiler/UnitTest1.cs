using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace StorySpoiler
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net/api";

      
     [OneTimeSetUp]
        public void Setup()
        {
          
            string token = GetJwtToken("nevenabal", "nevenabal");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Test, Order(1)]
        public void CreateStory_ShouldReturnCreated()
        {
            var story = new
            {
               Title = "New Story",
                Description = "Test story description",
                url = ""
            };

            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);
            Console.WriteLine(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdStoryId = json.GetProperty("storyId").GetString();
        }

        [Test, Order(2)]
        public void EditStoryTitle_ShouldReturnOk()
        {
            var changes = new
            {
                Title = "Updated Story Title",
                Description = "Updated Description",
                Url = ""
            };

            var request = new RestRequest($"/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);
            Console.WriteLine(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test, Order(3)]
        public void GetAll_Story_ShouldReturnList()
        {
            var request = new RestRequest("/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var story = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(story, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteStory_ShouldReturnOk()
        {
            var request = new RestRequest($"/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var story = new
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string fakeId = "123"; // несъществуващо ID

            var changes = new
            {
                Title = "New Story Title",
                Description = "Some description",
                Url = ""
            };

            var request = new RestRequest($"/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);
            Console.WriteLine(response.Content); 
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }


        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string fakeId = "555";
            var request = new RestRequest($"/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}