using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CustomerCare.Customers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomerInfoController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerInfoController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("{customerId:guid}/address")]
        public async Task<IActionResult> GetAddress(Guid customerId)
        {
            if (DateTime.Now.Second % 2 == 0)
            {
                return StatusCode(418, "Give me a piece of cake");
            }
            
            var address = await _customerService.GetAddressFor(customerId);
            if (string.IsNullOrEmpty(address))
            {
                return NotFound();
            }

            return Ok(address);
        }
    }
}