namespace OrderApi.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrder(Order order);
    }
}
