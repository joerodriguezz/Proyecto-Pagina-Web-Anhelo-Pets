using System.Data;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class VolunteerService : IVolunteerService
{
    private readonly AnheloPetsDbContext _dbContext;

    public VolunteerService(AnheloPetsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<VolunteerDto> GetAll()
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_volunteers_admin();");
        return ExecuteVolunteerReader(command);
    }

    public VolunteerDto? GetById(long id)
    {
        using var command = CreateCommand(
            "SELECT * FROM anhelopets.fn_get_volunteers_admin() WHERE volunteer_id = @id;");

        AddParameter(command, "id", id);
        return ExecuteVolunteerReader(command).FirstOrDefault();
    }

    public VolunteerDto Create(VolunteerDto volunteer)
    {
        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_register_volunteer(
                @userId::bigint,
                @nationalId::varchar,
                @volunteerType::varchar,
                @motivation::text,
                @createdBy::varchar);
            """);

        AddParameter(command, "userId", volunteer.UserId);
        AddParameter(command, "nationalId", volunteer.NationalId);
        AddParameter(command, "volunteerType", volunteer.VolunteerType);
        AddParameter(command, "motivation", volunteer.Motivation);
        AddParameter(command, "createdBy", volunteer.CreatedBy);

        var volunteerId = Convert.ToInt64(ExecuteScalar(command));
        return GetById(volunteerId) ?? throw new InvalidOperationException("Volunteer was created but could not be loaded.");
    }

    public VolunteerDto? Update(long id, VolunteerDto volunteer)
    {
        var existing = GetById(id);
        if (existing == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(volunteer.ValidationStatus))
        {
            using var validateCommand = CreateCommand(
                """
                SELECT anhelopets.fn_validate_volunteer(
                    @volunteerId::bigint,
                    @validationStatus::varchar,
                    @validatedByUserId::bigint,
                    @validationNotes::text,
                    @modifiedBy::varchar);
                """);

            AddParameter(validateCommand, "volunteerId", id);
            AddParameter(validateCommand, "validationStatus", volunteer.ValidationStatus);
            AddParameter(validateCommand, "validatedByUserId", volunteer.ValidatedByUserId);
            AddParameter(validateCommand, "validationNotes", volunteer.ValidationNotes);
            AddParameter(validateCommand, "modifiedBy", volunteer.ModifiedBy);
            ExecuteNonQuery(validateCommand);
        }

        if (volunteer.Active.HasValue)
        {
            using var activeCommand = CreateCommand(
                "SELECT anhelopets.fn_set_volunteer_active(@volunteerId::bigint, @active::boolean, @modifiedBy::varchar);");

            AddParameter(activeCommand, "volunteerId", id);
            AddParameter(activeCommand, "active", volunteer.Active.Value);
            AddParameter(activeCommand, "modifiedBy", volunteer.ModifiedBy);
            ExecuteNonQuery(activeCommand);
        }

        return GetById(id);
    }

    public bool Delete(long id)
    {
        if (GetById(id) == null)
        {
            return false;
        }

        using var command = CreateCommand(
            "SELECT anhelopets.fn_set_volunteer_active(@volunteerId::bigint, false, @modifiedBy::varchar);");

        AddParameter(command, "volunteerId", id);
        AddParameter(command, "modifiedBy", "api");
        ExecuteNonQuery(command);

        return true;
    }

    private List<VolunteerDto> ExecuteVolunteerReader(IDbCommand command)
    {
        try
        {
            using var reader = command.ExecuteReader();
            var volunteers = new List<VolunteerDto>();

            while (reader.Read())
            {
                volunteers.Add(new VolunteerDto
                {
                    VolunteerId = GetInt64(reader, "volunteer_id"),
                    UserId = GetInt64(reader, "user_id"),
                    FullName = GetString(reader, "full_name"),
                    NationalId = GetString(reader, "national_id"),
                    VolunteerType = GetString(reader, "volunteer_type"),
                    Motivation = GetString(reader, "motivation"),
                    Email = GetString(reader, "email"),
                    Phone = GetString(reader, "phone_primary"),
                    City = GetString(reader, "city"),
                    Town = GetString(reader, "town"),
                    Active = GetBoolean(reader, "active"),
                    ValidationStatus = GetString(reader, "validation_status"),
                    ValidationNotes = GetString(reader, "validation_notes"),
                    ValidatedAt = GetDateTime(reader, "validated_at"),
                    ValidatedByUserId = GetNullableInt64(reader, "validated_by_user_id")
                });
            }

            return volunteers;
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

    private static bool? GetBoolean(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
    }

    private static DateTime? GetDateTime(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
