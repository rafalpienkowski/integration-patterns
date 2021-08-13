using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerCare.Customers
{
    public interface ICustomerService
    {
        public Task Add(Customer customer);
        public Task<string> GetAddressFor(Guid customerId);
        public Task<List<Customer>> GetAll();
    }
}