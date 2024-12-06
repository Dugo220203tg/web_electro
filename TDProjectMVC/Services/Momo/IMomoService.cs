using TDProjectMVC.Models;
using TDProjectMVC.Models.MoMo;

namespace TDProjectMVC.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
