using System;
using System.ComponentModel.DataAnnotations;

namespace CustomerCare.Customers
{
    public class Customer
    {
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(255, ErrorMessage = "First name is too long")]
        public string FirstName { get; set; }
        
        [Required]
        [StringLength(255, ErrorMessage = "Last name is too long")]
        public string LastName { get; set; }
        
        [Required]
        [StringLength(255, ErrorMessage = "Address name is too long")]
        public string Address { get; set; }
    }
}