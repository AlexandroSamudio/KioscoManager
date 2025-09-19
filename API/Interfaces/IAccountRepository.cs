using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IAccountRepository
{
    Task<Result<UserDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken);
    Task<Result<UserDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken);
    Task<Result<UserDto>> CreateKioscoAsync(int userId, CreateKioscoDto createKioscoDto, CancellationToken cancellationToken);
    Task<Result<IReadOnlyList<GeneratedInvitationCodeDto>>> GetKioscoInvitationCodesAsync(int userId, CancellationToken cancellationToken);
    Task<Result<UserDto>> JoinKioscoAsync(int userId, JoinKioscoDto joinKioscoDto, CancellationToken cancellationToken);
    Task<Result<GeneratedInvitationCodeDto>> GenerateInvitationCodeAsync(int userId, CancellationToken cancellationToken);
}
