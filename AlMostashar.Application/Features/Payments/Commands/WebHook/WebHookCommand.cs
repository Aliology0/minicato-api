using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Domain.Shared;
using MediatR;

namespace AlMostashar.Application.Features.Payments.Commands.WebHook
{
    public class WebHookCommand: IRequest<Result<string>>, ITransactionalCommand
    {
        public string Hmac { get; set; } = string.Empty;
        public PaymobWebHookDto Payload { get; set; } = default!;
        public string GatewayResponse { get; set; } = null!;

    }

    public class PaymobWebHookDto
    {
        public string type { get; set; } = null!;
        public PaymobTransactionObjDto obj { get; set; } = null!;
    }

    public class PaymobTransactionObjDto
    {
        public int id { get; set; }
        public bool pending { get; set; }
        public int amount_cents { get; set; }
        public bool success { get; set; }
        public bool is_auth { get; set; }
        public bool is_capture { get; set; }
        public bool is_standalone_payment { get; set; }
        public bool is_voided { get; set; }
        public bool is_refunded { get; set; }
        public bool is_3d_secure { get; set; }
        public int integration_id { get; set; }
        public int profile_id { get; set; }
        public bool has_parent_transaction { get; set; }
        public PaymobOrderDto order { get; set; } = null!;
        public string created_at { get; set; } = null!;
        public string currency { get; set; } = null!;
        public PaymobSourceDataDto source_data { get; set; } = null!;
        public bool error_occured { get; set; }
        public int owner { get; set; }
        public decimal? merchant_commission { get; set; } 
        public decimal? refunded_amount_cents { get; set; } 

    }

    public class PaymobOrderDto
    {
        public int id { get; set; }
        public string created_at { get; set; } = null!;
        public string merchant_order_id { get; set; } = null!;
    }

    public class PaymobSourceDataDto
    {
        public string type { get; set; } = null!;
        public string pan { get; set; } = null!;
        public string sub_type { get; set; } = null!;
    }
}
