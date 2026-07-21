using AnheloPets.API.DTOs;
using AnheloPets.API.Exceptions;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class RescateService : IRescateService
{
    private readonly RescueRepository _repository;

    public RescateService(RescueRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RescateDto>> GetAll()
    {
        return await _repository.GetAll();
    }

    public async Task<RescateDto?> GetById(long id)
    {
        return await _repository.GetById(id);
    }

    public async Task<RescateDto> Create(RescateDto rescreate)
    {
        return await _repository.Create(rescreate);
    }

    public async Task<RescateDto?> Update(long id, RescateDto rescate)
    {
        return await _repository.Update(id, rescate);
    }

    public async Task<bool> Close(long id)
    {
        return await _repository.Close(id);
    }
}
