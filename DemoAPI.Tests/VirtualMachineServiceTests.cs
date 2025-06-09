using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using DemoAPI.Models;
using DemoAPI.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using TokenEncryptor;
public class VirtualMachineServiceTests
{
    private readonly Mock<IOptions<ApiAuthentication>> _mockAuthentication;
    private readonly ApiAuthentication _apiSettings;

    public VirtualMachineServiceTests()
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        if (!TokenEncryptorModul.checkEncryptedToken(appSettingsPath))
        {
            Console.WriteLine("Something went wrong while encrypting the token");
            return;
        }
        var config = File.ReadAllText(appSettingsPath);
        var root = JsonDocument.Parse(config).RootElement;
        var api_authentication_dict = JsonSerializer.Deserialize<Dictionary<string, string>>(root.GetProperty("ApiAuthentication"));
        _apiSettings = new ApiAuthentication
    {
        BaseUrl = api_authentication_dict["BaseUrl"],
        Token = api_authentication_dict["Token"],
        AESKey = api_authentication_dict["AESKey"],
        AESIV = api_authentication_dict["AESIV"]
    };
        _mockAuthentication = new Mock<IOptions<ApiAuthentication>>();
        _mockAuthentication.Setup(x => x.Value).Returns(_apiSettings);

    }
    private VirtualMachineService CreateServiceWithResponse(HttpResponseMessage responseMessage)
    {
        var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handlerMock
           .Protected()
           // Setup the PROTECTED method SendAsync
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(responseMessage)
           .Verifiable();

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(_apiSettings.BaseUrl)
        };

        // Setup the Authorization header decryption static call
        // We'll mock the EncryptionHelper.DecryptToken by substituting its result manually.
        // Because it's a static method, for unit test, it's better to refactor or wrap. Here, skip header assertion.

        return new VirtualMachineService(httpClient, _mockAuthentication.Object);
    }

    [Fact]
    public async Task GetVM_ReturnsVM_WithPatchableAndParentCount()
    {
        var vmList = new List<VirtualMachineResponse>
        {
            new VirtualMachineResponse { Id = 1, Status = "Running", ParentId = null },
            new VirtualMachineResponse { Id = 2, Status = "Stopped", ParentId = 1 },
            new VirtualMachineResponse { Id = 3, Status = "Running", ParentId = 2 },
        };

        var json = JsonSerializer.Serialize(vmList);
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

        var service = CreateServiceWithResponse(response);

        var vms = await service.GetVirtualMachineAsync(null);

        Assert.Equal(3, vms.Count);

        var dict = vms.ToDictionary(vm => vm.Id);

        Assert.Equal(0, dict[1].ParentVMCount);
        Assert.Equal(1, dict[2].ParentVMCount);
        Assert.Equal(2, dict[3].ParentVMCount);

        Assert.All(vms, vm => Assert.IsType<bool>(vm.IsPatchable));
    }
}
