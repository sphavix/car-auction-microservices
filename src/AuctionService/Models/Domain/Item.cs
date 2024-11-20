using System.ComponentModel.DataAnnotations.Schema;

namespace AuctionService.Api.Models.Domain
{
    [Table("Items")] // it's related to the auction table, no DbSet needed when this prop is declared
    public class Item
    {
        public Guid Id { get; set; }
        public  string Make { get; set; }
        public string  Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public int Mileage { get; set; }
        public string ImageUrl { get; set; }

        // navigation property. One-to-One Relationship
        public Auction Auction { get; set; }
        public Guid AuctionId { get; set; }
    }
    
}
