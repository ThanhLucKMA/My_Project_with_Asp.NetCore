using Microsoft.AspNetCore.Mvc;
using SolidEdu.Share;
using Ecommerce.WebApi.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.WebApi.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class CustomersController :ControllerBase
{
    private readonly ICustomerRepository repo;
    public CustomersController (ICustomerRepository repo)
    {
        this.repo = repo;
    }
    //GET: api/customers
    //GET: api/customers/?country=[country]
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<Customer>))]
    public async Task<IEnumerable<Customer>> GetCustomers (string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return await repo.RetrieveAllAsync();
        }
        else
        {
            return (await repo.RetrieveAllAsync())
                .Where(c => c.Country == country);
        }
    }
    //GET: api/customers/[id]
    [HttpGet("{id}",Name = nameof(GetCustomer))]
    [ProducesResponseType(200, Type = typeof(Customer))]
    [ProducesResponseType (404)]
    public async Task<IActionResult> GetCustomer(string id)
    {
        Customer? c = await repo.RetrieveAsync (id);
        if(c == null)
        {
            return NotFound ();
        }
        else
        {
            return Ok (c);
        }
    }
    //POST: api/customer/
    [HttpPost]
    [ProducesResponseType(200,Type = typeof(Customer))]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] Customer c)
    {
        if(c == null)
        {
            return BadRequest();
        }
        Customer? addedCustomer = await repo.CreateAsync(c);
        if(addedCustomer == null)
        {
            return BadRequest ("Repository faild to create customer" + c.CustomerId);
        }
        else
        {
            //problem details
            return CreatedAtRoute(
            routeName: nameof(GetCustomer),
            routeValues: new { id = addedCustomer.CustomerId.ToLower() },//anonymous type
            value: addedCustomer
            );
        }
    }
    //PUT: api/customers/update
    /*[HttpPut]
    [ProducesResponseType(200,Type = typeof(Customer))]
    [ProducesResponseType (400)]
    public async Task<IActionResult> Update ([FromBody] Customer c)
    {
        if (c == null || c.CustomerId == null)
        {
            return BadRequest ();
        }
        else
        {
                var check = await repo.RetrieveAsync(c.CustomerId);
                if(check != null)
                {
                    var res = await repo.UpdateAsync(c.CustomerId,c);
                    if (res == null) { return BadRequest (); }
                }
                else
                {
                    return BadRequest();
                }
            return Ok(c);
        } 
    }*/
    //PUT: api/customers/[id]
    [HttpPut("{id}")]
    [ProducesResponseType(202)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update (string id, [FromBody] Customer c)
    {
        id = id.ToUpper();
        c.CustomerId = c.CustomerId.ToUpper();
        if (id == null || c.CustomerId != id)
        {
            return BadRequest();//400
        }
        Customer existing = await repo.RetrieveAsync(id);
        if (existing == null)
        {
            return NotFound();//404
        }
        await repo.UpdateAsync(id, c);
        return new NoContentResult();
    }
    //DELETE: api/customers/[id]
    [HttpDelete("{id}")]
    [ProducesResponseType(202)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete (string id)
    {
        Customer? existing = await repo.RetrieveAsync(id);
        if(existing == null)
        {
            if (id =="bad")
            {
                ProblemDetails problemDetails = new()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://localhost:5001/customers/failed-to-delete",
                    Title = $"Customer ID {id} found but failed to delete.",
                    Detail = "More details like Company Name,Country and so on.",
                    Instance = HttpContext.Request.Path
                };
                return BadRequest(problemDetails);
            }
            return NotFound();
        }
        bool? deleted = await repo.DeleteAsync(id);
        if(deleted.HasValue && deleted.Value)
        {
            return new NoContentResult();
        }
        else
        {
            return BadRequest($"Customer {id} was not found but failed to delete...");
        }
    }
   
}
