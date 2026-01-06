using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using X39.Software.ExamMaker.Api.DataTransferObjects.Exams;
using X39.Software.ExamMaker.Api.Services;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Shared;

namespace X39.Software.ExamMaker.Api.Controllers;

[ApiController]
[Route("exam")]
public sealed class ExamController(ExamDbContext examDbContext, ILogger<ExamController> logger) : ControllerBase
{
    [HttpPut("{examId:guid}/emplace")]
    [ProducesResponseType<ExamListingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        if (existing is not null && existing.OrganizationFk != organizationId)
        {
            logger.LogWarning(
                "Unauthorized access attempt: User with organization {OrganizationId} tried to modify exam {ExamId} belonging to organization {ExamOrganizationId}",
                organizationId,
                examId,
                existing.OrganizationFk
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
            var now = SystemClock.Instance.GetCurrentInstant();
            await examDbContext.Exams.AddAsync(
                existing = new Exam
                {
                    Identifier     = examId,
                    OrganizationFk = organizationId,
                    CreatedAt      = now,
                    UpdatedAt      = now,
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

            using (UpdateTimeStampHelper.Create(existing))
            {
                if (examUpdateDto.Preamble is not null)
                    existing.Preamble = examUpdateDto.Preamble.Value;
                if (examUpdateDto.Title is not null)
                    existing.Title = examUpdateDto.Title.Value;
            }
        }

        logger.LogDebug("Saving changes to database for exam {ExamId}", examId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully created/updated exam {ExamId}", examId);
        return Ok(new ExamListingDto(examId, existing.Title, existing.Preamble, existing.CreatedAt.ToDateTimeOffset()));
    }

    [HttpGet("all")]
    [ProducesResponseType<ExamListingDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
            .Where(e => e.OrganizationFk == organizationId)
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            .Where(e => e.OrganizationFk == organizationId)
            .LongCountAsync(cancellationToken);

        logger.LogInformation("Total exam count for organization {OrganizationId}: {Count}", organizationId, result);
        return Ok(result);
    }

    [HttpGet("{examId:guid}")]
    [ProducesResponseType<ExamListingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSingleAsync(
        [FromRoute] Guid examId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("GetSingleAsync called for examId: {ExamId}", examId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to get exam {ExamId} - no organization ID found", examId);
            return Unauthorized();
        }

        logger.LogDebug("Querying exam {ExamId}", examId);
        var exam = await examDbContext.Exams
            .Where(e => e.Identifier == examId)
            .SingleOrDefaultAsync(cancellationToken);

        if (exam is null)
        {
            logger.LogInformation("Exam {ExamId} not found", examId);
            return Unauthorized();
        }

        if (exam.OrganizationFk != organizationId)
        {
            logger.LogWarning(
                "Unauthorized access to exam {ExamId} from organization {OrganizationId}",
                examId,
                organizationId
            );
            return Unauthorized();
        }

        logger.LogInformation("Returning exam {ExamId}", examId);
        return Ok(new ExamListingDto(exam.Identifier, exam.Title, exam.Preamble, exam.CreatedAt.ToDateTimeOffset()));
    }

    [HttpDelete("{examId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            return Unauthorized();
        }

        if (existing.OrganizationFk != organizationId)
        {
            logger.LogWarning(
                "Unauthorized deletion attempt: User with organization {OrganizationId} tried to delete exam {ExamId} belonging to organization {ExamOrganizationId}",
                organizationId,
                examId,
                existing.OrganizationFk
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

    [HttpGet("{examId:guid}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExamPdfAsync(
        [FromRoute] Guid examId,
        [FromQuery] bool showResults,
        [FromServices] PdfService pdfService,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("GetExamPdfAsync called for examId: {ExamId}", examId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to access exam {ExamId} - no organization ID found", examId);
            return Unauthorized();
        }

        logger.LogDebug(
            "Querying for exam {ExamId} to create a pdf from for organization {OrganizationId}",
            examId,
            organizationId
        );
        var existing = await examDbContext.Exams
            .AsNoTracking()
            .Include(e => e.ExamTopics!)
            .ThenInclude(e => e.ExamQuestions!)
            .ThenInclude(e => e.ExamAnswers!)
            .AsSplitQuery()
            .Where(e => e.Identifier == examId)
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            logger.LogInformation("Exam {ExamId} not found", examId);
            return Unauthorized();
        }

        if (existing.OrganizationFk != organizationId)
        {
            logger.LogWarning(
                "Unauthorized access attempt: User with organization {OrganizationId} tried to access exam {ExamId} belonging to organization {ExamOrganizationId}",
                organizationId,
                examId,
                existing.OrganizationFk
            );
            return Unauthorized();
        }

        logger.LogInformation("Creating exam {ExamId} PDF of organization {OrganizationId}", examId, organizationId);
        var pdfBytes = await pdfService.CreateExamPdfAsync(
            existing,
            showResults,
            CultureInfo.CurrentCulture,
            cancellationToken
        );

        return File(pdfBytes, "application/pdf", $"{existing.Title}.pdf");
    }
}
