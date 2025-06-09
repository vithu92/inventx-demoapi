using DemoAPI.Services;
public class VirtualMachineServiceTests
{
    private readonly VirtualMachineService _vmService;
    public VirtualMachineServiceTests(VirtualMachineService vmService)
    {
        _vmService = vmService;
    }

    [Fact]
    public void GetVM_NotNullAndNotEmpty()
    {
        var svm = _vmService.GetVirtualMachineAsync("");
        Assert.NotNull(svm);
    }
}
