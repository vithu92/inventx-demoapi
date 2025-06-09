public class VirtualMachineResponse : VirtualMachine
{
    public bool IsPatchable { get; set; }
    public int ParentVMCount { get; set; }
}