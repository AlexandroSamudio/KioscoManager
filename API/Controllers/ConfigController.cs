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

        [HttpGet("kiosco")]
        public async Task<ActionResult<KioscoConfigDto>> GetKioscoConfig(CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();

            var configDto = await _configRepository.GetKioscoConfigAsync(kioscoId, cancellationToken);
            return Ok(configDto);
        }

        [HttpPut("kiosco")]
        public async Task<ActionResult<KioscoConfigDto>> UpdateKioscoConfig(KioscoConfigUpdateDto configUpdateDto, CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();

            try
            {
                var configDto = await _configRepository.UpdateKioscoConfigAsync(kioscoId, configUpdateDto, cancellationToken);
                return Ok(configDto);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al actualizar la configuración: {ex.Message}");
            }
        }

        [HttpPut("kiosko/info-basico")]
        [Authorize(Roles = "administrador")]
        public async Task<ActionResult<KioscoBasicInfoDto>> UpdateKioscoBasicInfo(KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken = default)
        {
            var kioscoId = User.GetKioscoId();

            var responseDto = await _configRepository.UpdateKioscoBasicInfoAsync(kioscoId, updateDto, cancellationToken);
            return Ok(responseDto);
            
        }

        [HttpGet("user/preferencias")]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            var preferencesDto = await _configRepository.GetUserPreferencesAsync(userId, cancellationToken);
            return Ok(preferencesDto);
        }

        [HttpPut("user/preferencias")]
        public async Task<ActionResult<UserPreferencesDto>> UpdateUserPreferences(UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            var preferencesDto = await _configRepository.UpdateUserPreferencesAsync(userId, updateDto, cancellationToken);
            return Ok(preferencesDto);
        }
    }
}
