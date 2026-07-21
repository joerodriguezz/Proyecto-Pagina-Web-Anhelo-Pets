using AnheloPets.API.DTOs;

namespace AnheloPets.API.Services;

public interface IAnimalMedicalRecordService
{
    Task<List<AnimalMedicalRecordDto>> GetAll();
    Task<List<AnimalMedicalRecordDto>> GetByAnimal(string animalId);
    Task<AnimalMedicalRecordDto?> GetById(long id);
    Task<AnimalMedicalRecordDto> Create(AnimalMedicalRecordDto dto);
    Task<bool> Delete(long id);
}
