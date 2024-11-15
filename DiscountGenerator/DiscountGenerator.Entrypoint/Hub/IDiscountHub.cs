namespace DiscountGenerator.Entrypoint.Hub;

using Models;

public interface IDiscountHub
{
    Task GenerationResponse(bool startedGeneration);

    Task UseCodeResponse(bool success);
    
    Task ReceiveMessage(string message);
    
    Task ReceiveCodes(DiscountCodeList codes);
}