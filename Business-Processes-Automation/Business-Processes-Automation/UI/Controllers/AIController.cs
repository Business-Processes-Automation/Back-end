using Business_Processes_Automation.BLL.DTOs.AI;
using Business_Processes_Automation.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business_Processes_Automation.UI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/ai")]
    public class AIController : ControllerBase
    {
        private readonly IAIContentService _aiContentService;

        public AIController(
            IAIContentService aiContentService)
        {
            _aiContentService = aiContentService;
        }

        [HttpPost("generate-post-text")]
        public async Task<ActionResult<GeneratePostTextResponseDTO>>
            GeneratePostText(
                GeneratePostTextRequestDTO dto,
                CancellationToken cancellationToken)
        {
            var generatedText =
                await _aiContentService.GeneratePostTextAsync(
                    dto.Prompt,
                    cancellationToken);

            return Ok(new GeneratePostTextResponseDTO
            {
                GeneratedText = generatedText
            });
        }

        [HttpPost("generate-and-save")]
        public async Task<ActionResult<GeneratePostTextResponseDTO>>
    GenerateAndSave(
        GenerateAndSaveRequestDTO dto,
        CancellationToken cancellationToken)
        {
            var text = await _aiContentService.GenerateAndSaveAsync(
                dto.PostId,
                dto.Prompt,
                cancellationToken);

            return Ok(new GeneratePostTextResponseDTO
            {
                GeneratedText = text
            });
        }

    }
}
