using AnheloPets.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace AnheloPets.API.Services;

public interface IAnimalPhotoService
{
    Task<List<AnimalPhotoDto>> GetByAnimalId(string animalId);
    Task<AnimalPhotoDto> Upload(string animalId, IFormFile file, bool isPrimary);
    Task<AnimalPhotoDto> SetPrimary(long photoId);
    Task Delete(long photoId);
}
