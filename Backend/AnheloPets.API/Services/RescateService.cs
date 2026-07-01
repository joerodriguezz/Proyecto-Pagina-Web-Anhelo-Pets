using System.Data;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class RescateService : IRescateService
{
    private readonly AnheloPetsDbContext _dbContext;

    public RescateService(AnheloPetsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<RescateDto> GetAll()
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_rescues_admin();");
        return ExecuteRescueReader(command);
    }

    public RescateDto? GetById(long id)
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_rescues_admin() WHERE rescue_id = @id;");
        AddParameter(command, "id", id);
        return ExecuteRescueReader(command).FirstOrDefault();
    }

    public RescateDto Create(RescateDto rescate)
    {
        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_create_rescue(
                @animalId::bigint,
                @rescueDate::date,
                @location::text,
                @description::text,
                @status::varchar,
                @fosterHomeId::bigint,
                @createdBy::varchar);
            """);

        AddWriteParameters(command, rescate, includeModifiedBy: false);
        var rescueId = Convert.ToInt64(ExecuteScalar(command));
        return GetById(rescueId) ?? throw new InvalidOperationException("Rescue was created but could not be loaded.");
    }

    public RescateDto? Update(long id, RescateDto rescate)
    {
        if (GetById(id) == null)
        {
            return null;
        }

        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_update_rescue(
                @rescueId::bigint,
                @animalId::bigint,
                @rescueDate::date,
                @location::text,
                @description::text,
                @status::varchar,
                @fosterHomeId::bigint,
                @modifiedBy::varchar);
            """);

        AddParameter(command, "rescueId", id);
        AddWriteParameters(command, rescate, includeModifiedBy: true);
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

        existing.Status = "Cerrado";
        existing.ModifiedBy = "api";
        Update(id, existing);
        return true;
    }

    private void AddWriteParameters(IDbCommand command, RescateDto rescue, bool includeModifiedBy)
    {
        AddParameter(command, "animalId", rescue.AnimalId);
        AddParameter(command, "rescueDate", rescue.Fecha);
        AddParameter(command, "location", rescue.Ubicacion);
        AddParameter(command, "description", rescue.Descripcion);
        AddParameter(command, "status", string.IsNullOrWhiteSpace(rescue.Status) ? "Activo" : rescue.Status);
        AddParameter(command, "fosterHomeId", rescue.FosterHomeId);
        AddParameter(command, includeModifiedBy ? "modifiedBy" : "createdBy", includeModifiedBy ? rescue.ModifiedBy : rescue.CreatedBy);
    }

    private List<RescateDto> ExecuteRescueReader(IDbCommand command)
    {
        try
        {
            using var reader = command.ExecuteReader();
            var rescues = new List<RescateDto>();

            while (reader.Read())
            {
                rescues.Add(new RescateDto
            {
                RescateId      = GetInt64(reader, "rescue_id"),
                AnimalId       = GetNullableInt64(reader, "animal_id"),
                AnimalName     = GetString(reader, "animal_name"),
                Fecha          = GetDateOnly(reader, "rescue_date") ?? default,
                Ubicacion      = GetString(reader, "location"),
                Descripcion    = GetString(reader, "description"),
                Status         = GetString(reader, "status"),
                FosterHomeId   = GetNullableInt64(reader, "foster_home_id"),
                FosterHomeName = GetString(reader, "foster_home_name"),
                VolunteerName  = GetString(reader, "volunteer_name")
            });
            }

            return rescues;
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
        parameter.Value = value switch
        {
            null => DBNull.Value,
            DateOnly date => date.ToDateTime(TimeOnly.MinValue),
            _ => value
        };
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

    private static DateOnly? GetDateOnly(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal))
        {
            return null;
        }

        var value = reader.GetValue(ordinal);
        return value switch
        {
            DateOnly date => date,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            _ => null
        };
    }
}
