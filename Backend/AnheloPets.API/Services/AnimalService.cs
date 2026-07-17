using System.Data;
using AnheloPets.API.Controllers;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Repository;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class AnimalService : IAnimalService
{
    private readonly AnheloPetsDbContext _dbContext;
    private readonly AnimalRepository _animalRepository;

    public AnimalService(AnheloPetsDbContext dbContext, AnimalRepository animalRepository)
    {
        _dbContext = dbContext;
        _animalRepository = animalRepository;
    }

    public IEnumerable<AnimalDto> GetAll(string? species = null, string? status = "Disponible", string? search = null)
    {
        using var command = CreateCommand(
            "SELECT * FROM anhelopets.fn_get_pet_catalog(@species::text, @status::text, @search::text);");

        AddParameter(command, "species", species);
        AddParameter(command, "status", status);
        AddParameter(command, "search", search);

        return ExecuteAnimalReader(command);
    }

    public AnimalDto? GetById(long id)
    {
        using var command = CreateCommand(
            "SELECT * FROM anhelopets.fn_get_pet_catalog(NULL, 'Todos', NULL) WHERE animal_id = @id;");

        AddParameter(command, "id", id);

        return ExecuteAnimalReader(command).FirstOrDefault();
    }

    public async Task<GetResponse> Create(AnimalDto animal)
    {
        return await _animalRepository.Create(animal);
    }

    public AnimalDto? Update(long id, AnimalDto animal)
    {
        if (GetById(id) == null)
        {
            return null;
        }

        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_update_animal(
                @animalId::bigint,
                @species::varchar,
                @breed::varchar,
                @animalName::varchar,
                @animalStatus::varchar,
                @healthStatus::varchar,
                @birthDate::date,
                @sex::varchar,
                @description::text,
                @modifiedBy::varchar);
            """);

        AddParameter(command, "animalId", id);
        AddAnimalWriteParameters(command, animal, includeModifiedBy: true);
        ExecuteNonQuery(command);

        return GetById(id);
    }

    private void AddAnimalWriteParameters(IDbCommand command, AnimalDto animal, bool includeModifiedBy)
    {
        AddParameter(command, "species", animal.Species);
        AddParameter(command, "breed", animal.Breed);
        AddParameter(command, "animalName", animal.AnimalName);
        AddParameter(command, "animalStatus", string.IsNullOrWhiteSpace(animal.AnimalStatus) ? "Disponible" : animal.AnimalStatus);
        AddParameter(command, "healthStatus", string.IsNullOrWhiteSpace(animal.HealthStatus) ? "Pendiente" : animal.HealthStatus);
        AddParameter(command, "birthDate", animal.BirthDate);
        AddParameter(command, "sex", animal.Sex);
        AddParameter(command, "description", animal.Description);

        if (includeModifiedBy)
        {
            AddParameter(command, "modifiedBy", animal.ModifiedBy);
            return;
        }

        AddParameter(command, "photoUrl", animal.PhotoUrl);
        AddParameter(command, "photoDescription", animal.PhotoDescription);
        AddParameter(command, "createdBy", animal.CreatedBy);
    }

    private List<AnimalDto> ExecuteAnimalReader(IDbCommand command)
    {
        try
        {
            using var reader = command.ExecuteReader();
            var animals = new List<AnimalDto>();

            while (reader.Read())
            {
                animals.Add(new AnimalDto
                {
                    AnimalId = GetString(reader, "animal_id"),
                    AnimalName = GetString(reader, "animal_name"),
                    Species = GetString(reader, "species"),
                    Breed = GetString(reader, "breed"),
                    BirthDate = GetDateOnly(reader, "birth_date"),
                    AgeYears = GetInt32(reader, "age_years"),
                    AgeMonths = GetInt32(reader, "age_months"),
                    //Sex = GetString(reader, "sex"),
                    AnimalStatus = GetString(reader, "animal_status"),
                    HealthStatus = GetString(reader, "health_status"),
                    Description = GetString(reader, "description"),
                    PhotoUrl = GetString(reader, "photo_url")
                });
            }

            return animals;
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

    private static int? GetInt32(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
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
