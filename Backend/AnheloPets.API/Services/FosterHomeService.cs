using System.Data;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class FosterHomeService : IFosterHomeService
{
    private readonly AnheloPetsDbContext _dbContext;

    public FosterHomeService(AnheloPetsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<FosterHomeDto> GetAll()
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_foster_homes_admin();");
        return ExecuteReader(command);
    }

    public FosterHomeDto? GetById(long id)
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_foster_homes_admin() WHERE foster_home_id = @id;");
        AddParameter(command, "id", id);
        return ExecuteReader(command).FirstOrDefault();
    }

    public FosterHomeDto Create(FosterHomeDto fosterHome)
    {
        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_create_foster_home(
                @volunteerId::bigint,
                @name::varchar,
                @address::text,
                @phone::varchar,
                @responsible::varchar,
                @capacity::integer,
                @createdBy::varchar);
            """);

        AddWriteParameters(command, fosterHome, includeModifiedBy: false);
        var fosterHomeId = Convert.ToInt64(ExecuteScalar(command));
        return GetById(fosterHomeId) ?? throw new InvalidOperationException("Foster home was created but could not be loaded.");
    }

    public FosterHomeDto? Update(long id, FosterHomeDto fosterHome)
    {
        if (GetById(id) == null)
        {
            return null;
        }

        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_update_foster_home(
                @fosterHomeId::bigint,
                @volunteerId::bigint,
                @name::varchar,
                @address::text,
                @phone::varchar,
                @responsible::varchar,
                @capacity::integer,
                @active::boolean,
                @modifiedBy::varchar);
            """);

        AddParameter(command, "fosterHomeId", id);
        AddWriteParameters(command, fosterHome, includeModifiedBy: true);
        ExecuteNonQuery(command);
        return GetById(id);
    }

    public bool Delete(long id)
    {
        var existing = GetById(id);
        if (existing == null)
        {
            return false;
        }

        existing.Active = false;
        existing.ModifiedBy = "api";
        Update(id, existing);
        return true;
    }

    private static void AddWriteParameters(IDbCommand command, FosterHomeDto fosterHome, bool includeModifiedBy)
    {
        AddParameter(command, "volunteerId", fosterHome.VolunteerId);
        AddParameter(command, "name", fosterHome.Name);
        AddParameter(command, "address", fosterHome.Address);
        AddParameter(command, "phone", fosterHome.Phone);
        AddParameter(command, "responsible", fosterHome.Responsible);
        AddParameter(command, "capacity", fosterHome.Capacity);

        if (includeModifiedBy)
        {
            AddParameter(command, "active", fosterHome.Active);
            AddParameter(command, "modifiedBy", fosterHome.ModifiedBy);
            return;
        }

        AddParameter(command, "createdBy", fosterHome.CreatedBy);
    }

    private List<FosterHomeDto> ExecuteReader(IDbCommand command)
    {
        try
        {
            using var reader = command.ExecuteReader();
            var fosterHomes = new List<FosterHomeDto>();

            while (reader.Read())
            {
                fosterHomes.Add(new FosterHomeDto
                {
                    FosterHomeId = GetInt64(reader, "foster_home_id"),
                    VolunteerId = GetNullableInt64(reader, "volunteer_id"),
                    Name = GetString(reader, "name"),
                    Address = GetString(reader, "address"),
                    Phone = GetString(reader, "phone"),
                    Responsible = GetString(reader, "responsible"),
                    Capacity = GetInt32(reader, "capacity"),
                    Active = GetBoolean(reader, "active")
                });
            }

            return fosterHomes;
        }
        finally
        {
            command.Connection?.Close();
        }
    }

    private IDbCommand CreateCommand(string commandText)
    {
        var connection = _dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        var command = connection.CreateCommand();
        command.CommandText = commandText;
        command.CommandTimeout = 60;
        return command;
    }

    private static object? ExecuteScalar(IDbCommand command)
    {
        try
        {
            return command.ExecuteScalar();
        }
        finally
        {
            command.Connection?.Close();
        }
    }

    private static void ExecuteNonQuery(IDbCommand command)
    {
        try
        {
            command.ExecuteNonQuery();
        }
        finally
        {
            command.Connection?.Close();
        }
    }

    private static void AddParameter(IDbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static string GetString(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static long GetInt64(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.GetInt64(ordinal);
    }

    private static long? GetNullableInt64(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    private static int GetInt32(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    private static bool GetBoolean(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }
}
