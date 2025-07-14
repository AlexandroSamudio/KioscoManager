using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.DTOs;
using API.Extensions;
using API.Interfaces;

namespace API.Controllers
{
    public class ConfigController(IConfigRepository configRepository) : BaseApiController
    {
        private readonly IConfigRepository _configRepository = configRepository;

        protected int KioscoId => User.GetKioscoId();
        protected int UserId => User.GetUserId();

        [HttpGet("kiosko/info-basico")]
        public async Task<ActionResult<KioscoBasicInfoDto>> GetKioscoBasicInfo(CancellationToken cancellationToken)
        {
            var basicInfoDto = await _configRepository.GetKioscoBasicInfoAsync(KioscoId, cancellationToken);

            if (basicInfoDto == null)
            {
                return NotFound("Información básica del kiosco no encontrada.");
            }
            
            return Ok(basicInfoDto);
        }

        [HttpGet("kiosco")]
        public async Task<ActionResult<KioscoConfigDto>> GetKioscoConfig(CancellationToken cancellationToken)
        {
            var configDto = await _configRepository.GetKioscoConfigAsync(KioscoId, cancellationToken);
            return Ok(configDto);
        }

        [HttpPut("kiosco")]
        public async Task<IActionResult> UpdateKioscoConfig(KioscoConfigUpdateDto configUpdateDto, CancellationToken cancellationToken)
        {
            var result = await _configRepository.UpdateKioscoConfigAsync(KioscoId, configUpdateDto, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("kiosko/info-basico")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> UpdateKioscoBasicInfo(KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await _configRepository.UpdateKioscoBasicInfoAsync(KioscoId, updateDto, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("user/preferencias")]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(CancellationToken cancellationToken)
        {
            var preferencesDto = await _configRepository.GetUserPreferencesAsync(UserId, cancellationToken);
            return Ok(preferencesDto);
        }

        [HttpPut("user/preferencias")]
        public async Task<IActionResult> UpdateUserPreferences(UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await _configRepository.UpdateUserPreferencesAsync(UserId, updateDto, cancellationToken);
            return result.ToActionResult();
        }
    }
}
