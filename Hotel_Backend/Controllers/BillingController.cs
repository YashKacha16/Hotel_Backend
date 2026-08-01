using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hotel_Backend.Models;
using Hotel_Backend.Services;

namespace Hotel_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantBillDto>>> GetBills()
        {
            var bills = await _billingService.GetBillsAsync();
            return Ok(bills);
        }

        [HttpGet("room-bills")]
        public async Task<ActionResult<IEnumerable<RoomBill>>> GetRoomBills()
        {
            var bills = await _billingService.GetRoomBillsAsync();
            return Ok(bills);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantBillDto>> GetBill(int id)
        {
            var bill = await _billingService.GetBillByIdAsync(id);
            if (bill == null) return NotFound();
            return Ok(bill);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<RestaurantBillDto>> GenerateBill([FromBody] CreateBillDto createBillDto)
        {
            try
            {
                var bill = await _billingService.GenerateBillAsync(createBillDto);
                return CreatedAtAction(nameof(GetBill), new { id = bill.Id }, bill);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RestaurantBillDto>> UpdateBill(int id, [FromBody] UpdateBillDto updateBillDto)
        {
            try
            {
                var bill = await _billingService.UpdateBillAsync(id, updateBillDto);
                if (bill == null) return NotFound();
                return Ok(bill);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/pay")]
        public async Task<ActionResult<RestaurantBillDto>> PayBill(int id, [FromBody] PayBillDto payBillDto)
        {
            var bill = await _billingService.MarkAsPaidAsync(id, payBillDto);
            if (bill == null) return NotFound();
            return Ok(bill);
        }
    }
}
