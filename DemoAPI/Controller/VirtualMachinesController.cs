using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class VirtualMachineController : ControllerBase
{
    private readonly VirtualMachineService _vmService;
    public VirtualMachineController(VirtualMachineService vmService)
    {
        _vmService = vmService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status)
    {
        var vmList = await _vmService.GetVirtualMachineAsync(status);
        return Ok(vmList);
    }

}