using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Dto;
using server.Dto.Order;
using server.Entities;
using server.Interface.Service;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN,USER")]
    public class OrderController : ControllerBase
    {

        private readonly IOrderService orderService;
        private readonly IPaymentService paymentService;
        private readonly IMapper mapper;

        public OrderController(
            IOrderService orderService,
            IPaymentService paymentService,
            IMapper mapper
        )
        {
            this.orderService = orderService;
            this.paymentService = paymentService;
            this.mapper = mapper;
        }

        [HttpPost("CreateOrder")]
        public async Task<ActionResult<ResponseDto>> CreateOrder([FromBody] CreateOrderDTO order)
        {
            if (!Int32.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }
            var res = new ResponseDto();

            Order createdOrder = await orderService.CreateOrderAsync(userId, order.CartId, order.ShipToAddress);

            PaymentDetails details = await paymentService.InitializePayment(userId, createdOrder.Id, createdOrder.TotalPriceAfterDiscount);
            return Ok(res.success("Order Created Successfully", details));
        }

        [HttpGet("Get-all-orders")]
        public async Task<ActionResult<ResponseDto>> GetAllOrders()
        {
            if (!Int32.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }
            var orders = await orderService.GetOrdersAsync(userId);
            var orderDto = mapper.Map<List<GetUserOrdersDTO>>(orders);

            return Ok(orderDto);
        }

        [HttpGet("orderdetail/{orderId}")]
        public async Task<ActionResult> GetOrderDetail(int orderId)
        {
            if (!Int32.TryParse(User.FindFirst("UserId")?.Value, out int userId))
            {
                return Unauthorized();
            }
            OrderDetailDTO details = await orderService.GetOrderDetailAsync(orderId,userId);
            var orderDto = mapper.Map<OrderDto>(details.order);

            return Ok(new {order=orderDto,paymentDetails=details.paymentDetails,details.shippingAddress});
        }
    }
}