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

    private static readonly HashSet<string> AllowedFilterColumns = ["species", "animal_status", "breed", "sex", "animal_name", "health_status"];

    public IEnumerable<AnimalDto> GetAll(string? species = null, string? status = "Disponible", string? search = null, string? column = null, string? value = null)
    {
        if (!string.IsNullOrWhiteSpace(column) && !string.IsNullOrWhiteSpace(value))
        {
            if (!AllowedFilterColumns.Contains(column))
                throw new ArgumentException($"Column '{column}' is not allowed for filtering");

            using var dbCommand = CreateCommand(
                $"SELECT * FROM anhelopets.fn_get_pet_catalog(NULL, 'Todos', NULL) WHERE {column} = @filterValue");

            AddParameter(dbCommand, "filterValue", value);
            return ExecuteAnimalReader(dbCommand);
        }

        using var command = CreateCommand(
            "SELECT * FROM anhelopets.fn_get_pet_catalog(@species::text, @status::text, @search::text);");

        AddParameter(command, "species", species);
        AddParameter(command, "status", status);
        AddParameter(command, "search", search);

        return ExecuteAnimalReader(command);
    }

    public AnimalDto? GetById(string id)
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

    public async Task<AnimalDto?> ChangeStatus(string id, string status)
    {
        using var command = CreateCommand(
            "UPDATE anhelopets.animals SET animal_status = @status, modified_at = NOW(), modified_by = 'api' WHERE animal_id = @id");

        AddParameter(command, "id", id);
        AddParameter(command, "status", status);
        ExecuteNonQuery(command);

        return GetById(id);
    }

    public async Task<AnimalDto?> Update(string id, AnimalDto animal)
    {
        return await _animalRepository.Update(id, animal);
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
            AddParameter(command, "modifiedBy", "api");
            return;
        }

        AddParameter(command, "photoUrl", string.Empty);
        AddParameter(command, "photoDescription", string.Empty);
        AddParameter(command, "createdBy", "api");
    }

    private List<AnimalDto> ExecuteAnimalReader(IDbCommand command)
    {
        try
        {
            using var reader = command.ExecuteReader();
            var animals = new List<AnimalDto>();

            while (reader.Read())
            {
                var ageYears = GetInt32(reader, "age_years");
                var totalAgeMonths = GetInt32(reader, "age_months");

                animals.Add(new AnimalDto
                {
                    AnimalId = GetIdString(reader, "animal_id"),
                    AnimalName = GetString(reader, "animal_name"),
                    Species = GetString(reader, "species"),
                    Breed = GetString(reader, "breed"),
                    BirthDate = GetDateOnly(reader, "birth_date"),
                    AgeYears = ageYears,
                    // age_months de fn_get_pet_catalog es el total de meses desde el
                    // nacimiento (años*12 + meses); acá se guarda solo el resto tras
                    // los años completos, que es lo que el frontend concatena con AgeYears.
                    AgeMonths = totalAgeMonths.HasValue ? totalAgeMonths.Value - (ageYears ?? 0) * 12 : null,
                    Sex = GetString(reader, "sex"),
                    Size = GetString(reader, "size"),
                    AnimalStatus = GetString(reader, "animal_status"),
                    HealthStatus = GetString(reader, "health_status"),
                    Description = GetString(reader, "description"),
                    PhotoUrl = GetString(reader, "photo_url"),
                    Personality = GetString(reader, "personality")
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

    private static string GetIdString(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal)) return string.Empty;
        var value = reader.GetValue(ordinal);
        return value?.ToString() ?? string.Empty;
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
