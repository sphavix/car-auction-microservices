using AuctionService.Api.Models.Domain;
using AuctionService.Api.Models.Dtos;
using AuctionService.Api.Persistence;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Events.Auctions;

namespace AuctionService.Api.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint)); 
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAuctions(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            if(!String.IsNullOrEmpty(date))
            {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);

            // add current user as seller
            auction.Seller = User.Identity.Name;

            _context.Auctions.Add(auction);

            var response = _mapper.Map<AuctionDto>(auction);

            // publish the event to the event bus
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(response));

            var result = await _context.SaveChangesAsync() > 0;

            if (!result)
            {
                return BadRequest("An error occured while trying to save changes!");
            }

            return CreatedAtAction(nameof(GetAuction), new { auction.Id }, response);
        }

        [Authorize]
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
            var auction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);

            if(auction is null)
            {
                return NotFound();
            }

            // check seller  is the username that is updating
            if(auction.Seller != User.Identity.Name)
            {
                return Forbid();
            }

            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

            // publish the updated auction to the event bus.
            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

            var result = await _context.SaveChangesAsync() > 0;

            if(result)
            {
                return Ok();
            }

            return BadRequest("An error occured while saving changes!");
        }

        [Authorize]
        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction is null) return NotFound();

            if(auction.Seller != User.Identity.Name)
            {
                return Forbid();
            }

            _context.Auctions.Remove(auction);

            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("An error occured while trying to make changes!");

            return Ok();

        }
    }
}
