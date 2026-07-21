using AnheloPets.API.DTOs;
using AnheloPets.API.Repository;

namespace AnheloPets.API.Services;

public class AnimalMedicalRecordService : IAnimalMedicalRecordService
{
    private readonly AnimalMedicalRecordRepository _repo;

    public AnimalMedicalRecordService(AnimalMedicalRecordRepository repo)
    {
        _repo = repo;
    }

    public Task<List<AnimalMedicalRecordDto>> GetAll() => _repo.GetAll();
    public Task<List<AnimalMedicalRecordDto>> GetByAnimal(string animalId) => _repo.GetByAnimal(animalId);
    public Task<AnimalMedicalRecordDto?> GetById(long id) => _repo.GetById(id);
    public Task<AnimalMedicalRecordDto> Create(AnimalMedicalRecordDto dto) => _repo.Create(dto);
    public Task<bool> Delete(long id) => _repo.Delete(id);
}
