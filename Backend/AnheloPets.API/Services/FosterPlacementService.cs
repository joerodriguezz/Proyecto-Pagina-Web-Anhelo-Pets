using System.Data;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class FosterPlacementService : IFosterPlacementService
{
    private readonly AnheloPetsDbContext _dbContext;

    public FosterPlacementService(AnheloPetsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<FosterPlacementDto> GetAll()
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_foster_placements_admin();");
        return ExecuteReader(command);
    }

    public FosterPlacementDto? GetById(long id)
    {
        using var command = CreateCommand(
            "SELECT * FROM anhelopets.fn_get_foster_placements_admin() WHERE animal_foster_placement_id = @id;");
        AddParameter(command, "id", id);
        return ExecuteReader(command).FirstOrDefault();
    }

    public FosterPlacementDto Create(FosterPlacementDto placement)
    {
        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_assign_animal_foster_home(
                @animalId::bigint,
                @fosterHomeId::bigint,
                @startDate::date,
                @endDate::date,
                @notes::text,
                @createdBy::varchar);
            """);

        AddWriteParameters(command, placement, includeModifiedBy: false);
        var placementId = Convert.ToInt64(ExecuteScalar(command));
        return GetById(placementId) ?? throw new InvalidOperationException("Foster placement was created but could not be loaded.");
    }

    public FosterPlacementDto? Update(long id, FosterPlacementDto placement)
    {
        if (GetById(id) == null)
        {
            return null;
        }

        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_update_foster_placement(
                @placementId::bigint,
                @animalId::bigint,
                @fosterHomeId::bigint,
                @startDate::date,
                @endDate::date,
                @notes::text,
                @modifiedBy::varchar);
            """);

        AddParameter(command, "placementId", id);
        AddWriteParameters(command, placement, includeModifiedBy: true);
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

        existing.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        existing.ModifiedBy = "api";
        Update(id, existing);
        return true;
    }

    private static void AddWriteParameters(IDbCommand command, FosterPlacementDto placement, bool includeModifiedBy)
    {
        AddParameter(command, "animalId", placement.AnimalId);
        AddParameter(command, "fosterHomeId", placement.FosterHomeId);
        AddParameter(command, "startDate", placement.StartDate);
        AddParameter(command, "endDate", placement.EndDate);
        AddParameter(command, "notes", placement.Notes);
        AddParameter(command, includeModifiedBy ? "modifiedBy" : "createdBy", includeModifiedBy ? placement.ModifiedBy : placement.CreatedBy);
    }

    private List<FosterPlacementDto> ExecuteReader(IDbCommand command)
    {
        try
        {
            using var reader = command.ExecuteReader();
            var placements = new List<FosterPlacementDto>();

            while (reader.Read())
            {
                placements.Add(new FosterPlacementDto
                {
                    AnimalFosterPlacementId = GetInt64(reader, "animal_foster_placement_id"),
                    AnimalId = GetInt64(reader, "animal_id"),
                    AnimalName = GetString(reader, "animal_name"),
                    FosterHomeId = GetInt64(reader, "foster_home_id"),
                    FosterHomeName = GetString(reader, "foster_home_name"),
                    StartDate = GetDateOnly(reader, "start_date") ?? default,
                    EndDate = GetDateOnly(reader, "end_date"),
                    Notes = GetString(reader, "notes")
                });
            }

            return placements;
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
