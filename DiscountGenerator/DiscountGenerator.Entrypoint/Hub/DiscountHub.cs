namespace DiscountGenerator.Entrypoint.Hub;

using Application;
using Microsoft.AspNetCore.SignalR;
using Models;

public class DiscountHub : Hub<IDiscountHub>
{
    private readonly IDiscountService _discountService;
    
    public DiscountHub(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    public async Task GenerateCodes(GenerateDiscountRequest request)
    {
        if (request.Count < 1 || request.Count > 2000 || request.Length < 7 || request.Length > 8)
        {
            await Clients.Caller.GenerationResponse(false);
            await Clients.Caller.ReceiveMessage("Invalid request. Count must be between 1 and 2000. Length must be 7 or 8.");
            return;
        }

        try
        {
            var result = await _discountService.GenerateDiscountCodesAsync(request.Count, request.Length);
            await Clients.Caller.GenerationResponse(result);

            await SendCodesToCaller();
        }
        catch (Exception e)
        {
            await Clients.Caller.GenerationResponse(false);
            await Clients.Caller.ReceiveMessage(e.Message);
        }
    }
    
    public async Task UseCode(string code)
    {
        if (code.Length != 7 && code.Length != 8)
        {
            await Clients.Caller.UseCodeResponse(false);
            await Clients.Caller.ReceiveMessage("Invalid code. Code must be 7 or 8 characters long.");
            return;
        }

        try
        {
            var result = await _discountService.UpdateCodeAsync(code);
            await Clients.Caller.UseCodeResponse(result);
            
            await SendCodesToCaller();
        }
        catch (Exception e)
        {
            await Clients.Caller.UseCodeResponse(false);
            await Clients.Caller.ReceiveMessage(e.Message);
        }
    }
    
    public async Task GetCodes(GetCodesRequest request)
    {
        try
        {
            await SendCodesToCaller(request.PageNumber, request.PageSize);
        }
        catch (Exception e)
        {
            await Clients.Caller.ReceiveMessage(e.Message);
        }
    }
    
    private async Task SendCodesToCaller(int pageNumber = 1, int pageSize = 10)
    {
        var discountCodes = await _discountService.GetDiscountCodesAsync(pageNumber, pageSize);
        var discountCodeList = new DiscountCodeList(
            discountCodes.Data.Select(d => new DiscountCodeResponse(code: d.Code, isUsed: d.IsUsed)).ToList(),
            discountCodes.PageInfo);

        await Clients.Caller.ReceiveCodes(discountCodeList);
    }
    
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.ReceiveMessage($"Connection successful, ID: {Context.ConnectionId}");
        
        await SendCodesToCaller();
        
        await base.OnConnectedAsync();
    }
}