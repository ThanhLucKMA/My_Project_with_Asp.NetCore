using Microsoft.EntityFrameworkCore.ChangeTracking;
using SolidEdu.Share;
using System.Collections.Concurrent;//use ConcurrentDictionary

namespace Ecommerce.WebApi.Repositories;
public class CustomerRepository : ICustomerRepository
{

    //Save information customer_ use static dictionary field to cache the customer
    private static ConcurrentDictionary<string, Customer> customerCache;

    //Use instance data context field
    private SolidStoreContext db;

    public CustomerRepository (SolidStoreContext injecttedContext)
    {
        this.db = injecttedContext;
        if(customerCache == null)
        {
            customerCache = new ConcurrentDictionary<string, Customer>(
               db.Customers.ToDictionary(c => c.CustomerId));
        }
    }

    public async Task<Customer> CreateAsync(Customer c)
    {
        c.CustomerId = c.CustomerId.ToUpper();
        EntityEntry<Customer> added = await db.Customers.AddAsync(c);
        int affected = await db.SaveChangesAsync();
        if (affected == 1)
        {
            if (customerCache is null) return c;
            else customerCache.AddOrUpdate(c.CustomerId, c, UpdateCache);
        }
        return null;
    }

    public async Task<bool?> DeleteAsync(string id)
    {
        id = id.ToUpper();
        //Delete in database 
        Customer? c = db.Customers.Find(id);
        if (c != null)
        {
            db.Customers.Remove(c);
            int effected = await db.SaveChangesAsync();
            if (effected == 1)
            {
                if (customerCache is null) return null;
                //remove from cache 
                return customerCache.TryRemove(id, out c);
            }
            else
            {
                return null;
            }
        }
        return null;
    }

    public Task<IEnumerable<Customer>> RetrieveAllAsync()
    {
        return Task.FromResult(customerCache is null? 
            Enumerable.Empty<Customer>() : customerCache.Values);
    }

    public Task<Customer> RetrieveAsync(string id)
    {
        id = id.ToUpper();
        if (customerCache is null) return null!;
        customerCache.TryGetValue(id, out Customer? c);
        return Task.FromResult(c);
    }

    public async Task<Customer> UpdateAsync(string id, Customer c)
    {
        id = id.ToUpper();
        c.CustomerId = c.CustomerId.ToUpper();
        //update in database by EF Core
        db.Customers.Update(c);
        int effected = await db.SaveChangesAsync(); 
        if(effected == 1)
        {
            //update in cache 
            return UpdateCache(id, c);
        }
        return null;
    }

    Task<bool> ICustomerRepository.DeleteAsync(string id)
    {
        throw new NotImplementedException();
    }

    private Customer UpdateCache (string id, Customer c)
    {
        Customer? old;
        if (customerCache is not null)
        {
            if(customerCache.TryGetValue(id, out old))
            {
                if(customerCache.TryUpdate(id, c, old))
                {
                    return c;
                }
            }
        }
        return null;
    }
}