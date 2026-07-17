using System.Data;
using System.Security.Cryptography;
using AnheloPets.API.Data;
using AnheloPets.API.DTOs;
using AnheloPets.API.Repository;
using Microsoft.EntityFrameworkCore;

namespace AnheloPets.API.Services;

public class UserService : IUserService
{
    private const int HashIterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private readonly AuthRepository _authRepository;

    private readonly AnheloPetsDbContext _dbContext;

    public UserService(AnheloPetsDbContext dbContext, AuthRepository authRepository)
    {
        _dbContext = dbContext;
        _authRepository = authRepository;
    }

    public async Task<AuthUserDto> Register(RegisterUserDto request)
    {
        request.Username = request.Email.Split("@")[0];
        request.LastName = request.FirstName.Split(" ")[1];
        request.Password = HashPassword(request.Password);
        return await _authRepository.Register(request);
    }
    

    public async Task<LoginDtoResponse> Login (LoginDtoRequest request)
    {
        AuthUserDto authUser = await _authRepository.Login(request);

        if (authUser == null)
        {
            throw new ApplicationException($"User {request.Email} not found");
        }

        if (!VerifyPassword(request.Password, authUser.Password))
        {
            return new LoginDtoResponse
            {
                Email = authUser.Email,
                Message = "Datos incorrectos"
            };
        }

        return new LoginDtoResponse
        {
            Email = authUser.Email,
            Message = "Datos correctos"
        };




    }

    public bool UpdatePassword(long userId, PasswordUpdateDto request)
    {
        if (GetAuthUserById(userId) == null)
        {
            return false;
        }

        using var command = CreateCommand(
            """
            SELECT anhelopets.fn_update_password_hash(
                @userId::bigint,
                @passwordHash::text,
                @modifiedBy::varchar);
            """);

        AddParameter(command, "userId", userId);
        AddParameter(command, "passwordHash", HashPassword(request.Password));
        AddParameter(command, "modifiedBy", request.ModifiedBy);
        ExecuteNonQuery(command);

        return true;
    }

    private AuthUserDto? GetAuthUser(string usernameOrEmail)
    {
        using var command = CreateCommand("SELECT * FROM anhelopets.fn_get_auth_user(@usernameOrEmail::text);");
        AddParameter(command, "usernameOrEmail", usernameOrEmail);

        try
        {
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadAuthUser(reader) : null;
        }
        finally
        {
            command.Connection?.Close();
        }
    }

    private AuthUserDto? GetAuthUserById(long userId)
    {
        using var command = CreateCommand(
            """
            SELECT *
            FROM anhelopets.fn_get_auth_user(
                (SELECT username FROM anhelopets.users WHERE user_id = @userId::bigint));
            """);

        AddParameter(command, "userId", userId);

        try
        {
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadAuthUser(reader) : null;
        }
        finally
        {
            command.Connection?.Close();
        }
    }

    private static AuthUserDto ReadAuthUser(IDataRecord reader)
    {
        return new AuthUserDto
        {
            UserId = GetString(reader, "user_id"),
            Username = GetString(reader, "username"),
            Email = GetString(reader, "email"),
            IsVolunteer = GetBoolean(reader, "is_volunteer"),
            VolunteerActive = GetBoolean(reader, "volunteer_active"),
            VolunteerValidationStatus = GetString(reader, "volunteer_validation_status"),
            Roles = GetStringArray(reader, "roles")
        };
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

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashIterations, HashAlgorithmName.SHA256, KeySize);

        return $"pbkdf2${HashIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != "pbkdf2" || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedHash = Convert.FromBase64String(parts[3]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
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

    private static bool GetBoolean(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }

    private static string[] GetStringArray(IDataRecord reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? [] : (string[])reader.GetValue(ordinal);
    }
}
