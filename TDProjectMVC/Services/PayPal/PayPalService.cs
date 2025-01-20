using PayPal.Core;
using PayPal.v1.Payments;
using System.Net;
using TDProjectMVC.ViewModels;

namespace TDProjectMVC.Services.PayPal
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _configuration;
        private readonly PayPalHttpClient _payPalClient;

        public PayPalService(IConfiguration config)
        {
            _configuration = config ?? throw new ArgumentNullException(nameof(config));
            var environment = new SandboxEnvironment(
                _configuration["Paypal:ClientId"] ?? throw new InvalidOperationException("PayPal ClientId is not configured."),
                _configuration["Paypal:SecretKey"] ?? throw new InvalidOperationException("PayPal SecretKey is not configured.")
            );
            _payPalClient = new PayPalHttpClient(environment);
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            if (collections == null)
                throw new ArgumentNullException(nameof(collections));

            var response = new PaymentResponseModel();
            var mappings = new Dictionary<string, Action<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["order_description"] = value => response.OrderDescription = value,
                ["transaction_id"] = value => response.TransactionId = value,
                ["order_id"] = value => response.OrderId = value,
                ["payment_method"] = value => response.PaymentMethod = value,
                ["success"] = value => response.Success = Convert.ToInt32(value) > 0,
                ["paymentid"] = value => response.PaymentId = value
            };

            foreach (var (key, value) in collections)
            {
                if (string.IsNullOrEmpty(key)) continue;
                if (mappings.TryGetValue(key, out var action))
                {
                    action(value!);
                }
            }

            return response;
        }

        public async Task<string> CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var paypalOrderId = DateTime.Now.Ticks;
            var urlCallBack = _configuration["PaymentCallBack:ReturnPayPalUrl"]
                ?? throw new InvalidOperationException("PaymentCallBack:ReturnUrl is not configured.");

            var payment = new Payment
            {
                Intent = "sale",
                Transactions = new List<Transaction>
                {
                    CreateTransaction(model, paypalOrderId)
                },
                RedirectUrls = CreateRedirectUrls(urlCallBack, paypalOrderId),
                Payer = new Payer
                {
                    PaymentMethod = "paypal"
                }
            };

            var request = new PaymentCreateRequest();
            request.RequestBody(payment);

            try
            {
                var response = await _payPalClient.Execute(request);

                if (response.StatusCode is not (HttpStatusCode.Accepted or HttpStatusCode.OK or HttpStatusCode.Created))
                    return string.Empty;

                var result = response.Result<Payment>();
                return result.Links.FirstOrDefault(lnk =>
                    lnk.Rel.Equals("approval_url", StringComparison.OrdinalIgnoreCase))?.Href ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Log the exception here
                return string.Empty;
            }
        }

        private static Transaction CreateTransaction(PaymentInformationModel model, long paypalOrderId)
        {
            var amount = ConvertVndToDollar(model.Amount).ToString();

            return new Transaction
            {
                Amount = new Amount
                {
                    Total = amount,
                    Currency = "USD",
                    Details = new AmountDetails
                    {
                        Tax = "0",
                        Shipping = "0",
                        Subtotal = amount
                    }
                },
                ItemList = new ItemList
                {
                    Items = new List<Item>
                    {
                        new Item
                        {
                            Name = $" | Order: {model.Description}",
                            Currency = "USD",
                            Price = amount,
                            Quantity = "1",
                            Sku = "sku",
                            Tax = "0",
                            Url = "https://www.code-mega.com"
                        }
                    }
                },
                Description = $"Invoice #{model.Description}",
                InvoiceNumber = paypalOrderId.ToString()
            };
        }

        private static RedirectUrls CreateRedirectUrls(string urlCallBack, long paypalOrderId)
        {
            return new RedirectUrls
            {
                ReturnUrl = $"{urlCallBack}?payment_method=PayPal&success=1&order_id={paypalOrderId}",
                CancelUrl = $"{urlCallBack}?payment_method=PayPal&success=0&order_id={paypalOrderId}"
            };
        }
        public static double ConvertVndToDollar(double vnd)
        {
            var total = Math.Round(vnd / 25, 2);

            return total;
        }
    }
}
