using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<int> DoSomethingAsync(int IdProduct, int IdWarehouse, int Amount, DateTime CreatedAt)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            //1
            if (Amount>0)
            {
                command.CommandText = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
                command.Parameters.AddWithValue("@IdProduct", IdProduct);

                var result = await command.ExecuteScalarAsync();

                if (result == null)
                {
                    throw new Exception("Product id not found");
                }

                await command.ExecuteNonQueryAsync();

                command.Parameters.Clear();
            
                command.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                command.Parameters.AddWithValue("@IdWarehouse", IdWarehouse);

                var result2 = await command.ExecuteScalarAsync();

                if (result2 == null)
                {
                    throw new Exception("Warehouse id not found");
                }

            }
            else
            {
                throw new Exception("Amount = 0");
            }
            //2
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM [Order] WHERE IdProduct = @IdProduct and Amount = @Amount and CreatedAt < @CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", IdProduct);
            command.Parameters.AddWithValue("@Amount", Amount);
            command.Parameters.AddWithValue("@CreatedAt", CreatedAt);
            var result3 = await command.ExecuteScalarAsync();
            if (result3 == null)
            {
                throw new Exception("Order not found");
            }
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            //3
            command.CommandText = "SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct and Amount = @Amount and CreatedAt < @CreatedAt";
            command.Parameters.AddWithValue("@IdProduct", IdProduct);
            command.Parameters.AddWithValue("@Amount", Amount);
            command.Parameters.AddWithValue("@CreatedAt", CreatedAt);
            var result4 = await command.ExecuteScalarAsync();
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            command.CommandText = "Select IdOrder from Product_Warehouse where IdOrder = @IdOrder ";
            command.Parameters.AddWithValue("@IdOrder",(int)result4);
            var result5 = await command.ExecuteScalarAsync();
            if (result5 != null)
            {
                throw new Exception("Zrealizowane przypadkiem ");
            }
            await command.ExecuteNonQueryAsync();
            //4
            command.Parameters.Clear();
            DateTime date = DateTime.Now;
            command.CommandText = "Update [Order] set FulfilledAt = @FulfilledAt where IdOrder = @IdOrder";
            command.Parameters.AddWithValue("@FulfilledAt",date);
            command.Parameters.AddWithValue("@IdOrder", (int)result4);
            await command.ExecuteNonQueryAsync();
            //5
            command.Parameters.Clear();
            command.CommandText="select price from Product where IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", IdProduct);
            var result6 = await command.ExecuteScalarAsync();
            await command.ExecuteNonQueryAsync();
            command.Parameters.Clear();
            command.CommandText = "INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, CreatedAt, Price) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @CreatedAt, @Price); SELECT SCOPE_IDENTITY()";
            command.Parameters.AddWithValue("@IdWarehouse", IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", IdProduct);
            command.Parameters.AddWithValue("@IdOrder", (int)result4);
            command.Parameters.AddWithValue("@Amount", Amount);
            command.Parameters.AddWithValue("@CreatedAt", date);
            var cena = Amount * (decimal)result6;
            command.Parameters.AddWithValue("@Price", cena);
            await command.ExecuteNonQueryAsync();
            
            //6
            var insertedIdObj = await command.ExecuteScalarAsync();
            int insertedId = Convert.ToInt32(insertedIdObj);
            await transaction.CommitAsync();
            return insertedId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }
    public async Task<int> task2withprocedure(int idProduct, int idWarehouse, int amount, DateTime createdAt)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using var command = new SqlCommand("AddProductToWarehouse", connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@IdProduct", idProduct);
        command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
        command.Parameters.AddWithValue("@Amount", amount);
        command.Parameters.AddWithValue("@CreatedAt", createdAt);

        await connection.OpenAsync();
    
        var result = await command.ExecuteScalarAsync();
    
        if (result == null )
        {
            throw new Exception("Procedure did not return an ID.");
        }

        return Convert.ToInt32(result);
    }
}