using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using OAuthServer.V2.Core.Common;
using OAuthServer.V2.Core.Repositories;
using OAuthServer.V2.Core.Services;
using OAuthServer.V2.Core.UnitOfWork;
using System.Linq.Expressions;
using System.Net;

namespace OAuthServer.V2.Service.Services;

public class ServiceGeneric<TEntity, TDto>(

    IMapper mapper,
    IUnitOfWork unitOfWork,
    IGenericRepository<TEntity> repository) : IServiceGeneric<TEntity, TDto> where TEntity : class where TDto : class
{

    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IGenericRepository<TEntity> _repository = repository;


    public async Task<ServiceResult<IEnumerable<TDto>>> GetAllAsync()
    {
        var dtos = _mapper.Map<List<TDto>>(await _repository.GetAll().ToListAsync());

        return ServiceResult<IEnumerable<TDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
    {
        var list = await _repository.Where(predicate).ToListAsync();

        var dtos = _mapper.Map<IEnumerable<TDto>>(list);

        return ServiceResult<IEnumerable<TDto>>.Success(dtos);
    }

    public async ValueTask<ServiceResult<TDto>> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);

        if (entity is null)
        {
            return ServiceResult<TDto>.Fail("ID NOT FOUND!", HttpStatusCode.NotFound);
        }

        var dto = _mapper.Map<TDto>(entity);

        return ServiceResult<TDto>.Success(dto);
    }

    public async Task<ServiceResult<TDto>> AddAsync(TDto dto)
    {
        // GELEN DTO NESNESİ ENTITY'E DÖNÜŞTÜRÜLÜR
        var entity = _mapper.Map<TEntity>(dto);

        // BU "entity" VARIABLE TÜM ALANLARI DOLDURULMUŞ ŞEKİLDE MEMORY'DE YENİ HALİ İLE DURUR.
        await _repository.AddAsync(entity);

        // DEĞİŞİKLİKLER VERİTABANINA KAYDEDİLİR
        await _unitOfWork.CommitAsync();

        // YENİ DEĞERLERİ İLE EKLENEN "entity" VARIABLE'INI TEKRAR DTO'YA DÖNÜŞTÜRÜYORUZ
        var newDto = _mapper.Map<TDto>(entity);

        return ServiceResult<TDto>.Success(newDto, HttpStatusCode.Created);
    }

    public async Task<ServiceResult> Update(TDto dto, int id)
    {
        // CHECK ENTITY
        var isExistEntity = await _repository.GetByIdAsync(id);

        if (isExistEntity is null)
        {
            return ServiceResult.Fail("ID NOT FOUND!", HttpStatusCode.NotFound);
        }

        var entity = _mapper.Map<TEntity>(isExistEntity);

        _repository.Update(entity);

        await _unitOfWork.CommitAsync();

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult> Delete(int id)
    {
        // CHECK ENTITY
        var isExistEntity = await _repository.GetByIdAsync(id);

        if (isExistEntity is null)
        {
            return ServiceResult.Fail("ID NOT FOUND!", HttpStatusCode.NotFound);
        }

        _repository.Delete(isExistEntity);

        await _unitOfWork.CommitAsync();

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

}
