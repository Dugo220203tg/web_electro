using API_Web_Shop_Electronic_TD.Models;

namespace API_Web_Shop_Electronic_TD.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
