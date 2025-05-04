namespace Tutorial9.Services;

public interface IDbService
{
    Task<int> DoSomethingAsync(int Idproduct, int IdWarehouse, int Amount, DateTime CreatedAt);
    Task<int> task2withprocedure(int idProduct, int idWarehouse, int amount, DateTime createdAt);
}