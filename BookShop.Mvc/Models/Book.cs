using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookShop.Mvc.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [Range(0.01d, 10000d)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }
        public List<OrderLine> OrderLines { get; set; }
    }
}