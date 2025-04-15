namespace Foodie.Repositories.For_DeliveryPartner
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAllData();
        T GetDataById(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
    }
}
