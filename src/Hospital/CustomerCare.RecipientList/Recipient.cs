using System;

namespace CustomerCare.RecipientList
{
    public class Recipient
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Queue { get; set; }
    }
}