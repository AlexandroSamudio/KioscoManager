using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using API.DTOs;
using API.Extensions;
using API.Interfaces;

namespace API.Controllers
{
    [Authorize]
    public class ConfigController(IConfigRepository configRepository) : BaseApiController
    {
        protected int KioscoId => User.GetKioscoId();
        protected int UserId => User.GetUserId();

        /// <summary>
        /// Obtiene la información básica del kiosco
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <returns>Información básica del kiosco como nombre, dirección, etc.</returns>
        /// <response code="200">Información básica del kiosco obtenida exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
        /// <response code="404">Kiosco no encontrado</response>
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("kiosko/info-basico")]
        [ProducesResponseType(typeof(KioscoBasicInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<KioscoBasicInfoDto>> GetKioscoBasicInfo(CancellationToken cancellationToken)
        {
            var result = await configRepository.GetKioscoBasicInfoAsync(KioscoId, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene la configuración completa del kiosco
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <returns>Configuración completa del kiosco incluyendo opciones de negocio</returns>
        /// <response code="200">Configuración del kiosco obtenida exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("kiosco")]
        [ProducesResponseType(typeof(KioscoConfigDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<KioscoConfigDto>> GetKioscoConfig(CancellationToken cancellationToken)
        {
            var result = await configRepository.GetKioscoConfigAsync(KioscoId, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Actualiza la configuración del kiosco
        /// </summary>
        /// <param name="configUpdateDto">Datos de configuración a actualizar</param>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <returns>Configuración del kiosco actualizada</returns>
        /// <response code="200">Configuración del kiosco actualizada exitosamente</response>
        /// <response code="400">Datos de configuración no válidos</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
        /// <response code="404">Configuración del kiosco no encontrada</response>
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("kiosco")]
        [ProducesResponseType(typeof(KioscoConfigDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<KioscoConfigDto>> UpdateKioscoConfig(KioscoConfigUpdateDto configUpdateDto, CancellationToken cancellationToken)
        {
            var result = await configRepository.UpdateKioscoConfigAsync(KioscoId, configUpdateDto, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Actualiza la información básica del kiosco
        /// </summary>
        /// <param name="updateDto">Datos básicos del kiosco a actualizar</param>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <returns>Información básica del kiosco actualizada</returns>
        /// <response code="200">Información básica del kiosco actualizada exitosamente</response>
        /// <response code="400">Datos no válidos</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="403">Prohibido. Se requieren permisos de administrador.</response>
        /// <response code="404">Kiosco no encontrado</response>
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPut("kiosko/info-basico")]
        [ProducesResponseType(typeof(KioscoBasicInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<KioscoBasicInfoDto>> UpdateKioscoBasicInfo(KioscoBasicInfoUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await configRepository.UpdateKioscoBasicInfoAsync(KioscoId, updateDto, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Obtiene las preferencias del usuario actual
        /// </summary>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <returns>Preferencias personales del usuario autenticado</returns>
        /// <response code="200">Preferencias del usuario obtenidas exitosamente</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        [Authorize]
        [HttpGet("user/preferencias")]
        [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserPreferencesDto>> GetUserPreferences(CancellationToken cancellationToken)
        {
            var result = await configRepository.GetUserPreferencesAsync(UserId, cancellationToken);
            return result.ToActionResult();
        }

        /// <summary>
        /// Actualiza las preferencias del usuario actual
        /// </summary>
        /// <param name="updateDto">Nuevas preferencias del usuario</param>
        /// <param name="cancellationToken">Token para cancelar la operación</param>
        /// <returns>Preferencias del usuario actualizadas</returns>
        /// <response code="200">Preferencias del usuario actualizadas exitosamente</response>
        /// <response code="400">Datos de preferencias no válidos</response>
        /// <response code="401">No autorizado. Se requiere un JWT válido.</response>
        /// <response code="404">Preferencias del usuario no encontradas</response>
        [Authorize]
        [HttpPut("user/preferencias")]
        [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetailsDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserPreferencesDto>> UpdateUserPreferences(UserPreferencesUpdateDto updateDto, CancellationToken cancellationToken)
        {
            var result = await configRepository.UpdateUserPreferencesAsync(UserId, updateDto, cancellationToken);
            return result.ToActionResult();
        }
    }
}
