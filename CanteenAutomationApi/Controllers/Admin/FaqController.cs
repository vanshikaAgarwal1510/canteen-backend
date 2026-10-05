using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CanteenBackend.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/faq")]
public class FaqController : ControllerBase
{
    private readonly AppDbContext _db;

    public FaqController(AppDbContext db)
    {
        _db = db;
    }
    [HttpPost("get-faqList")]
    public async Task<IActionResult> GetFaqList(GetFaqRequest request)
    {
        if (request.ApiKey != Constants.api)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid API key",
                data = (object?)null
            });
        }
        var faqs = await _db.Faqs.Where(u=>!u.isDeleted)
            .Select(u => new AddFaqResponse
            {
               Id = u.Id,
               Question = u.Question,
               Answer = u.Answer,
            })
            .ToListAsync();

        if (faqs.Count == 0)
            return NotFound(new
            {
                status = 404,
                message = "Faq's not found",
                data = (object?)null
            });

        return Ok(new
        {
            status = 200,
            message = "Faq's list retrieved successfully",
            data = faqs
        });
    }

    [HttpPost("add-faq")]
    public async Task<IActionResult> AddEditFaq(AddEditFaqRequest request)
    {
        if (request.ApiKey != Constants.api)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid API key",
                data = (object?)null
            });
        }

     

        //add
        if (request.Type == 1)
        
        {
            FAQ? existingFaq = await _db.Faqs
    .FirstOrDefaultAsync(u => u.Question.ToLower() == request.Question!.ToLower());
          
        if (existingFaq!=null  )
            return BadRequest(new
            {
                status = 400,
                message = "Question already exists",
                data = (object?)null
            });


         var faq = new FAQ
        {
            Question = request.Question!,
            Answer= request.Answer!,
            createdAt = DateTime.UtcNow,
            isDeleted = false
        };


        _db.Faqs.Add(faq);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Faq added successfully",
            data = new AddFaqResponse
            {
               Id = faq.Id,
               Question  =faq.Question,
               Answer = faq.Answer
            }

        });
  
        }
       
       //update
        else 
        {
          FAQ? existingFaq = await _db.Faqs
        .FirstOrDefaultAsync(u => u.Id == request.Id);

            if (existingFaq == null)
            {
                return  NotFound (new
                {
                    status=404,
                    message = "Faq not found",
                    data = (object?)null
                });
            }

            existingFaq.Question = request.Question!;
            existingFaq.Answer = request.Answer!;

            await _db.SaveChangesAsync();

        return Ok(new
        {
            status = 200,
            message = "Faq updated successfully",
            data = new AddFaqResponse
            {
               Id = existingFaq.Id,
               Question  =existingFaq.Question,
               Answer = existingFaq.Answer
            }

        });
        
        }
   
    }
  
    [HttpPost("delete-faq")]
    public async Task<IActionResult> DeleteFaq([FromBody] DeleteFaqRequest request)
    {
        if (request.ApiKey != Constants.api)
        {
            return Unauthorized(new
            {
                status = 401,
                message = "Invalid API key",
                data = (object?)null
            });
        }
        var faq = await _db.Faqs
       .FirstOrDefaultAsync(u => u.Id == request.Id);

        if (faq == null)
            return NotFound(new
            {
                status = 404,
                message = "Faq not found",
                data = (object?)null
            });



        faq.isDeleted =true;

        await _db.SaveChangesAsync();


        return Ok(new
        {
            status = 200,
            message = "Faq deleted successfully",
            data = (object?)null
        });
    }
}

public class GetFaqRequest
{
    public required string ApiKey { get; set; }
}
public class AddEditFaqRequest
{
    public required string ApiKey { get; set; }
     public  string? Question{get; set;}
    public  string? Answer{get; set;}
    public int Type{get; set;}//type 1 ->add type 2->update

    public int Id{get; set;}

}

public class AddFaqResponse
{
    public required int Id{get; set;}
    public required string Question{get; set;}
    public required string Answer{get; set;}

}
public class DeleteFaqRequest
{
    public required string ApiKey { get; set; }
     public required int Id{get; set;}



}