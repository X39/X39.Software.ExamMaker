using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using X39.Software.ExamMaker.Api.DataTransferObjects.Exams;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Shared;

namespace X39.Software.ExamMaker.Api.Controllers;

[ApiController]
[Route("exam")]
public sealed class ExamController(ExamDbContext examDbContext, ILogger<ExamController> logger) : ControllerBase
{
    [HttpPut("{examId:guid}/emplace")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateOrUpdateAsync(
        [FromRoute] Guid examId,
        ExamUpdateDto examUpdateDto,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("CreateOrUpdateAsync called with examId: {ExamId}", examId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to create/update exam {ExamId} - no organization ID found",
                examId
            );
            return Unauthorized();
        }

        logger.LogDebug(
            "Querying for existing exam {ExamId} for organization {OrganizationId}",
            examId,
            organizationId
        );
        var existing = await examDbContext.Exams
            .Where(e => e.Identifier == examId)
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is not null && existing.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized access attempt: User with organization {OrganizationId} tried to modify exam {ExamId} belonging to organization {ExamOrganizationId}",
                organizationId,
                examId,
                existing.OrganizationId
            );
            return Unauthorized();
        }

        if (existing is null)
        {
            logger.LogInformation(
                "Creating new exam {ExamId} for organization {OrganizationId}",
                examId,
                organizationId
            );
            await examDbContext.Exams.AddAsync(
                new Exam
                {
                    Identifier     = examId,
                    OrganizationId = organizationId,
                    CreatedAt      = SystemClock.Instance.GetCurrentInstant(),
                    Preamble       = examUpdateDto.Preamble ?? string.Empty,
                    Title          = examUpdateDto.Title ?? string.Empty,
                },
                cancellationToken
            );
        }
        else
        {
            logger.LogInformation(
                "Updating existing exam {ExamId} for organization {OrganizationId}",
                examId,
                organizationId
            );
            logger.LogDebug(
                "Update details - Preamble provided: {PreambleProvided}, Title provided: {TitleProvided}",
                examUpdateDto.Preamble is not null,
                examUpdateDto.Title is not null
            );

            if (examUpdateDto.Preamble is not null)
                existing.Preamble = examUpdateDto.Preamble;
            if (examUpdateDto.Title is not null)
                existing.Title = examUpdateDto.Title;
        }

        logger.LogDebug("Saving changes to database for exam {ExamId}", examId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully created/updated exam {ExamId}", examId);
        return NoContent();
    }

    [HttpGet("all")]
    [ProducesResponseType<ExamListingDto[]>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery, Required, Range(0, int.MaxValue)] int skip,
        [FromQuery, Required, Range(1, 100)] int take,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("GetAllAsync called with skip: {Skip}, take: {Take}", skip, take);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to list exams - no organization ID found");
            return Unauthorized();
        }

        if (skip < 0 || take < 0)
        {
            logger.LogWarning("Invalid pagination parameters: skip={Skip}, take={Take}", skip, take);
            return BadRequest("Skip and take must be greater than 0");
        }

        if (take > 100)
        {
            logger.LogWarning("Take parameter exceeds maximum: {Take}", take);
            return BadRequest("Take must be less than 100");
        }

        logger.LogDebug(
            "Querying exams for organization {OrganizationId} with pagination skip={Skip}, take={Take}",
            organizationId,
            skip,
            take
        );
        var query = examDbContext.Exams
            .Where(e => e.OrganizationId == organizationId)
            .OrderBy(e => e.Title);
        var data = await query.Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} exams for organization {OrganizationId}", data.Count, organizationId);
        return Ok(
            data.Select(e => new ExamListingDto(e.Identifier, e.Title, e.Preamble, e.CreatedAt.ToDateTimeOffset()))
        );
    }

    [HttpGet("all/count")]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCountAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("GetAllCountAsync called");

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to get exam count - no organization ID found");
            return Unauthorized();
        }

        logger.LogDebug("Counting exams for organization {OrganizationId}", organizationId);
        var result = await examDbContext.Exams
            .Where(e => e.OrganizationId == organizationId)
            .LongCountAsync(cancellationToken);

        logger.LogInformation("Total exam count for organization {OrganizationId}: {Count}", organizationId, result);
        return Ok(result);
    }

    [HttpGet("{examId:guid}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid examId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("DeleteAsync called for examId: {ExamId}", examId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to delete exam {ExamId} - no organization ID found", examId);
            return Unauthorized();
        }

        logger.LogDebug(
            "Querying for exam {ExamId} to delete for organization {OrganizationId}",
            examId,
            organizationId
        );
        var existing = await examDbContext.Exams
            .Where(e => e.Identifier == examId)
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            logger.LogInformation("Exam {ExamId} not found for deletion", examId);
            return NoContent();
        }

        if (existing.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized deletion attempt: User with organization {OrganizationId} tried to delete exam {ExamId} belonging to organization {ExamOrganizationId}",
                organizationId,
                examId,
                existing.OrganizationId
            );
            return NoContent();
        }

        logger.LogInformation("Deleting exam {ExamId} for organization {OrganizationId}", examId, organizationId);
        examDbContext.Exams.Remove(existing);

        logger.LogDebug("Saving deletion changes for exam {ExamId}", examId);
        await examDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully deleted exam {ExamId}", examId);
        return NoContent();
    }
}
