using Business_Processes_Automation.BLL.DTOs.Post;
using Business_Processes_Automation.BLL.Interfaces;
using Business_Processes_Automation.UI.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Business_Processes_Automation.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    [HttpPost]
    public async Task<ActionResult<PostResponseDTO>> Create(
        [FromBody] CreatePostRequestDTO dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var created = await _postService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponseDTO>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var post = await _postService.GetByIdAsync(id, cancellationToken);
        if (post is null)
        {
            return NotFound(new { message = ApiPostsMessages.PostNotFound });
        }

        return Ok(post);
    }

    [HttpGet("master/{masterId:int}")]
    public async Task<ActionResult<IReadOnlyList<PostResponseDTO>>> GetAllByMaster(
        int masterId,
        CancellationToken cancellationToken)
    {
        var posts = await _postService.GetAllByMasterAsync(masterId, cancellationToken);
        return Ok(posts);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _postService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { message = ApiPostsMessages.PostNotFound });
        }

        return NoContent();
    }
}
