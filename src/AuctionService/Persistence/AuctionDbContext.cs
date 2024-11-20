using AuctionService.Api.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Api.Persistence
{
    public class AuctionDbContext : DbContext
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }


        public DbSet<Auction> Auctions { get; set; }
    }
}
