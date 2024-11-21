using AuctionService.Api.Models.Domain;
using AuctionService.Api.Models.Dtos;
using AuctionService.Api.Persistence;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Api.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
        {
            var auctions = await _context.Auctions.Include(x => x.Item).OrderBy(x => x.Item.Make).ToListAsync();

            var response = _mapper.Map<List<AuctionDto>>(auctions);

            return Ok(response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if (auction is null)
            {
                return NotFound();
            }

            var response = _mapper.Map<AuctionDto>(auction);

            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var aution = _mapper.Map<Auction>(auctionDto);

            // TODO: add current user as seller
            aution.Seller = "Test";

            _context.Auctions.Add(aution);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("An error occured while trying to save changes!");
            }

            var response = _mapper.Map<AuctionDto>(aution);

            return CreatedAtAction(nameof(GetAuction), new { aution.Id }, response);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if(auction is null)
            {
                return NotFound();
            }

            // TODO: check seller  is the username that is updating

            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if(result)
            {
                return Ok();
            }

            return BadRequest("An error occured while saving changes!");
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction is null) return NotFound();

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("An error occured while trying to make changes!");

            return Ok();

        }
    }
}
