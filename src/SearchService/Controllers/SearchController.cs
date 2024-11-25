using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Api.Models.Domain;
using SearchService.Api.RequestHelpers;

namespace SearchService.Api.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item, Item>();

            if(!String.IsNullOrEmpty(searchParams.SearchString))
            {
                query.Match(Search.Full, searchParams.SearchString).SortByTextScore();
            }

            // orderby and filterby
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(x => x.Make)),
                "new" => query.Sort(x => x.Descending(x => x.CreatedAt)),
                _ => query.Sort(x => x.Ascending(x => x.AuctionEnd))
            };

            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if(!String.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }

            if (!String.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x => x.Seller == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}
