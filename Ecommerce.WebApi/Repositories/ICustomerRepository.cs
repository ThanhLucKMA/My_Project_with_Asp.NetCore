using SolidEdu.Share;

namespace Ecommerce.WebApi.Repositories;

public interface ICustomerRepository
{
    Task<Customer> CreateAsync (Customer c);//Tao
    Task<IEnumerable<Customer>> RetrieveAllAsync ();//Truy xuat tat ca
    Task<Customer> RetrieveAsync(string id);//Lay Customer theo id
    Task<Customer> UpdateAsync(string id, Customer c);//Update customer theo id
    Task<bool> DeleteAsync(string id);

}