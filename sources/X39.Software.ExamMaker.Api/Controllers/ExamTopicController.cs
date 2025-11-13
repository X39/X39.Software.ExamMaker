using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using X39.Software.ExamMaker.Api.DataTransferObjects.ExamTopics;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Shared;

namespace X39.Software.ExamMaker.Api.Controllers;

[ApiController]
[Route("exam/{examId:guid}/topic")]
public sealed class ExamTopicController(ExamDbContext examDbContext, ILogger<ExamTopicController> logger)
    : ControllerBase
{
    [HttpPut("{topicId:guid}/emplace")]
    [ProducesResponseType<ExamTopicListingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrUpdateAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        ExamTopicUpdateDto updateDto,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "CreateOrUpdateAsync (Topic) called with examId: {ExamId}, topicId: {TopicId}",
            examId,
            topicId
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to create/update topic {TopicId} for exam {ExamId} - no organization ID found",
                topicId,
                examId
            );
            return Unauthorized();
        }

        logger.LogDebug("Loading exam {ExamId} for org {OrganizationId}", examId, organizationId);
        var exam = await examDbContext.Exams
            .Where(e => e.Identifier == examId)
            .SingleOrDefaultAsync(cancellationToken);
        if (exam is null)
        {
            logger.LogWarning("Exam {ExamId} not found when creating/updating topic {TopicId}", examId, topicId);
            return Unauthorized();
        }

        if (exam.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized: Organization {OrganizationId} tried to modify topic {TopicId} under exam {ExamId} belonging to organization {ExamOrgId}",
                organizationId,
                topicId,
                examId,
                exam.OrganizationId
            );
            return Unauthorized();
        }

        logger.LogDebug("Querying for existing topic {TopicId}", topicId);
        var existing = await examDbContext.ExamTopics
            .Where(t => t.Identifier == topicId)
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is not null && existing.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized: Organization {OrganizationId} tried to modify topic {TopicId} belonging to organization {TopicOrgId}",
                organizationId,
                topicId,
                existing.OrganizationId
            );
            return Unauthorized();
        }

        if (existing is null)
        {
            logger.LogInformation("Creating new topic {TopicId} under exam {ExamId}", topicId, examId);
            var now = SystemClock.Instance.GetCurrentInstant();
            await examDbContext.ExamTopics.AddAsync(
                existing = new ExamTopic
                {
                    Identifier           = topicId,
                    OrganizationId       = organizationId,
                    CreatedAt            = now,
                    UpdatedAt            = now,
                    Exam                 = exam,
                    Title                = updateDto.Title ?? string.Empty,
                    QuestionAmountToTake = updateDto.QuestionAmountToTake,
                },
                cancellationToken
            );
        }
        else
        {
            logger.LogInformation("Updating existing topic {TopicId} under exam {ExamId}", topicId, examId);
            // No updatable fields provided in ExamTopicUpdateDto; placeholder for future updates
            if (existing.Exam?.Identifier != examId)
            {
                logger.LogWarning("Topic {TopicId} does not belong to exam {ExamId}", topicId, examId);
                return Unauthorized();
            }

            using (UpdateTimeStampHelper.Create(existing))
            {
                if (updateDto.Title is not null)
                    existing.Title = updateDto.Title.Value;
                if (updateDto.QuestionAmountToTake is not null)
                    existing.QuestionAmountToTake = updateDto.QuestionAmountToTake.Value;
            }
        }

        logger.LogDebug("Saving changes for topic {TopicId}", topicId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully created/updated topic {TopicId}", topicId);
        return Ok(
            new ExamTopicListingDto(
                existing.Identifier,
                existing.Title,
                existing.QuestionAmountToTake,
                existing.CreatedAt.ToDateTimeOffset()
            )
        );
    }

    [HttpGet("all")]
    [ProducesResponseType<ExamTopicListingDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync(
        [FromRoute] Guid examId,
        [FromQuery, Required, Range(0, int.MaxValue)] int skip,
        [FromQuery, Required, Range(1, 100)] int take,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "GetAllAsync (Topics) called with examId: {ExamId}, skip: {Skip}, take: {Take}",
            examId,
            skip,
            take
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to list topics for exam {ExamId} - no organization ID found",
                examId
            );
            return Unauthorized();
        }

        if (skip < 0 || take < 0)
        {
            logger.LogWarning("Invalid pagination parameters for topics: skip={Skip}, take={Take}", skip, take);
            return BadRequest("Skip and take must be greater than 0");
        }

        if (take > 100)
        {
            logger.LogWarning("Take parameter exceeds maximum for topics: {Take}", take);
            return BadRequest("Take must be less than 100");
        }

        logger.LogDebug(
            "Querying topics for org {OrganizationId}, exam {ExamId} with pagination skip={Skip}, take={Take}",
            organizationId,
            examId,
            skip,
            take
        );
        var query = examDbContext.ExamTopics
            .Where(t => t.OrganizationId == organizationId && t.Exam!.Identifier == examId)
            .OrderBy(t => t.Title);
        var data = await query.Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Retrieved {Count} topics for exam {ExamId}", data.Count, examId);
        return Ok(
            data.Select(e => new ExamTopicListingDto(
                        e.Identifier,
                        e.Title,
                        e.QuestionAmountToTake,
                        e.CreatedAt.ToDateTimeOffset()
                    )
                )
                .ToArray()
        );
    }

    [HttpGet("all/count")]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllCountAsync(
        [FromRoute] Guid examId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("GetAllCountAsync (Topics) called with examId: {ExamId}", examId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to count topics for exam {ExamId} - no organization ID found",
                examId
            );
            return Unauthorized();
        }

        logger.LogDebug("Counting topics for org {OrganizationId}, exam {ExamId}", organizationId, examId);
        var count = await examDbContext.ExamTopics
            .Where(t => t.OrganizationId == organizationId && t.Exam!.Identifier == examId)
            .LongCountAsync(cancellationToken);
        logger.LogInformation("Total topic count for exam {ExamId}: {Count}", examId, count);
        return Ok(count);
    }

    [HttpGet("{topicId:guid}")]
    [ProducesResponseType<ExamTopicListingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSingleAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "GetSingleAsync (Topic) called with examId: {ExamId}, topicId: {TopicId}",
            examId,
            topicId
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to get topic {TopicId} for exam {ExamId} - no organization ID found",
                topicId,
                examId
            );
            return Unauthorized();
        }

        var topic = await examDbContext.ExamTopics
            .Include(t => t.Exam)
            .Where(t => t.Identifier == topicId)
            .SingleOrDefaultAsync(cancellationToken);

        if (topic is null)
        {
            logger.LogInformation("Topic {TopicId} not found", topicId);
            return Unauthorized();
        }

        if (topic.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized access to topic {TopicId} from organization {OrganizationId}",
                topicId,
                organizationId
            );
            return Unauthorized();
        }

        if (topic.Exam?.Identifier != examId)
        {
            logger.LogWarning("Topic {TopicId} does not belong to exam {ExamId}", topicId, examId);
            return Unauthorized();
        }

        return Ok(
            new ExamTopicListingDto(
                topic.Identifier,
                topic.Title,
                topic.QuestionAmountToTake,
                topic.CreatedAt.ToDateTimeOffset()
            )
        );
    }

    [HttpDelete("{topicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("DeleteAsync (Topic) called with examId: {ExamId}, topicId: {TopicId}", examId, topicId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to delete topic {TopicId} for exam {ExamId} - no organization ID found",
                topicId,
                examId
            );
            return Unauthorized();
        }

        logger.LogDebug("Querying topic {TopicId} including exam for deletion", topicId);
        var existing = await examDbContext.ExamTopics
            .Include(t => t.Exam)
            .Where(t => t.Identifier == topicId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            logger.LogInformation("Topic {TopicId} not found for deletion", topicId);
            return Unauthorized();
        }

        if (existing.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized deletion attempt: Org {OrganizationId} tried to delete topic {TopicId} belonging to org {TopicOrg}",
                organizationId,
                topicId,
                existing.OrganizationId
            );
            return NoContent();
        }

        if (existing.Exam?.Identifier != examId)
        {
            logger.LogWarning("Topic {TopicId} does not belong to exam {ExamId}", topicId, examId);
            return NoContent();
        }

        logger.LogInformation("Deleting topic {TopicId}", topicId);
        examDbContext.ExamTopics.Remove(existing);
        logger.LogDebug("Saving deletion changes for topic {TopicId}", topicId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully deleted topic {TopicId}", topicId);
        return NoContent();
    }
}
