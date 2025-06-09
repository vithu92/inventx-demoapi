using System.Net.Http.Headers;
using System.Text.Json;
using DemoAPI.Models;
using DemoAPI.Services;
using Microsoft.Extensions.Options;

public class VirtualMachineService
{
    private readonly HttpClient _httpClient;
    private ApiAuthentication _apiSettings;

    public VirtualMachineService(HttpClient httpClient, IOptions<ApiAuthentication> apiOptions)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(apiOptions.Value.BaseUrl);
        _apiSettings = apiOptions.Value;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", EncryptionHelper.DecryptToken(_apiSettings.Token, _apiSettings.AESKey, _apiSettings.AESIV));
    }

    public async Task<List<VirtualMachineResponse>> GetVirtualMachineAsync(string? statusFilter)
    {
        var response = await _httpClient.GetAsync("virtualmachines");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawVMs = JsonSerializer.Deserialize<List<VirtualMachineResponse>>(json, options);

        var vms = rawVMs?.Where(vm => vm != null).ToList() ?? new();
        var vmDict = vms.ToDictionary(vm => vm.Id);

        foreach (var vm in vms)
        {
            vm.IsPatchable = isPatchable(vm);
            vm.ParentVMCount = GetParentVMCount(vm, vmDict);
        }

        if (!string.IsNullOrEmpty(statusFilter))
        {
            vms = vms.Where(vm => string.Equals(vm.Status.ToLower(), statusFilter.ToLower(), StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return vms;
    }

    private bool isPatchable(VirtualMachineResponse vm)
    {
        var today = DateTime.Now;
        return today.Day == 3 && string.Equals(vm.Status, "Running", StringComparison.OrdinalIgnoreCase);
    }

    private int GetParentVMCount(VirtualMachineResponse vm, Dictionary<int, VirtualMachineResponse> vmDict)
    {
        int count = 0;
        var processed = new HashSet<int>();
        int? currentId = vm.ParentId;

        while (currentId != null && vmDict.ContainsKey(currentId.Value) && !processed.Contains(currentId.Value))
        {
            processed.Add(currentId.Value);
            count++;
            currentId = vmDict[currentId.Value].ParentId;
        }

        return count;
    }
}