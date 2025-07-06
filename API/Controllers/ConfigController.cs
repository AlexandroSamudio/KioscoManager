using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.DTOs;
using API.Extensions;
using API.Interfaces;

namespace API.Controllers
{
    public class ConfigController : BaseApiController
    {
        private readonly IConfigRepository _configRepository;

        public ConfigController(IConfigRepository configRepository)
        {
            _configRepository = configRepository;
        }

        [HttpGet("kiosko/info-basico")]
        public async Task<ActionResult<KioscoBasicInfoDto>> GetKioscoBasicInfo(CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();

            var basicInfoDto = await _configRepository.GetKioscoBasicInfoAsync(kioscoId, cancellationToken);

            if (basicInfoDto == null)
            {
                return NotFound("Información básica del kiosco no encontrada.");
            }
            
            return Ok(basicInfoDto);
        }

        [HttpGet("kiosco")]
        public async Task<ActionResult<KioscoConfigDto>> GetKioscoConfig(CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();

            var configDto = await _configRepository.GetKioscoConfigAsync(kioscoId, cancellationToken);
            return Ok(configDto);
        }

        [HttpPut("kiosco")]
        public async Task<IActionResult> UpdateKioscoConfig(KioscoConfigUpdateDto configUpdateDto, CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();

            await _configRepository.UpdateKioscoConfigAsync(kioscoId, configUpdateDto, cancellationToken);
            return NoContent();
        }

        [HttpPut("kiosko/info-basico")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<KioscoBasicInfoDto>> UpdateKioscoBasicInfo(KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var kioscoId = User.GetKioscoId();

            await _configRepository.UpdateKioscoBasicInfoAsync(kioscoId, updateDto, cancellationToken);
            return NoContent();
        }

        [HttpGet("user/preferencias")]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            var preferencesDto = await _configRepository.GetUserPreferencesAsync(userId, cancellationToken);
            return Ok(preferencesDto);
        }

        [HttpPut("user/preferencias")]
        public async Task<ActionResult<UserPreferencesDto>> UpdateUserPreferences(UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var userId = User.GetUserId();

            await _configRepository.UpdateUserPreferencesAsync(userId, updateDto, cancellationToken);
            return NoContent();
        }
    }
}
