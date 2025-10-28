using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using X39.Software.ExamMaker.Api.DataTransferObjects.ExamAnswers;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Shared;

namespace X39.Software.ExamMaker.Api.Controllers;

[ApiController]
[Route("exam/{examId:guid}/topic/{topicId:guid}/question/{questionId:guid}/answer")]
public sealed class ExamAnswerController(ExamDbContext examDbContext, ILogger<ExamAnswerController> logger)
    : ControllerBase
{
    [HttpPut("{answerId:guid}/emplace")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateOrUpdateAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        [FromRoute] Guid answerId,
        ExamAnswerUpdateDto examUpdateDto,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("CreateOrUpdateAsync (Answer) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}, answerId: {AnswerId}", examId, topicId, questionId, answerId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to create/update answer {AnswerId} - no organization ID found", answerId);
            return Unauthorized();
        }

        logger.LogDebug("Querying for question {QuestionId} including topic and exam for org {OrganizationId}", questionId, organizationId);
        var question = await examDbContext.ExamQuestions
            .Include(q => q.ExamTopic)!.ThenInclude(t => t!.Exam)
            .Where(q => q.Identifier == questionId)
            .SingleOrDefaultAsync(cancellationToken);
        if (question is null)
        {
            logger.LogWarning("Question {QuestionId} not found when creating/updating answer {AnswerId}", questionId, answerId);
            return Unauthorized();
        }
        if (question.ExamTopic?.Identifier != topicId || question.ExamTopic?.Exam?.Identifier != examId)
        {
            logger.LogWarning("Parent mismatch for answer {AnswerId}: topicId={TopicId}, examId={ExamId}", answerId, topicId, examId);
            return Unauthorized();
        }
        if (question.OrganizationId != organizationId)
        {
            logger.LogWarning("Unauthorized: Organization {OrganizationId} tried to modify answer {AnswerId} under question {QuestionId}", organizationId, answerId, questionId);
            return Unauthorized();
        }

        logger.LogDebug("Querying for existing answer {AnswerId}", answerId);
        var existing = await examDbContext.ExamAnswers
            .Where(a => a.Identifier == answerId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing is not null && existing.OrganizationId != organizationId)
        {
            logger.LogWarning("Unauthorized: Organization {OrganizationId} tried to modify answer {AnswerId} belonging to another org {OtherOrg}", organizationId, answerId, existing.OrganizationId);
            return Unauthorized();
        }

        if (existing is null)
        {
            logger.LogInformation("Creating new answer {AnswerId} under question {QuestionId}", answerId, questionId);
            await examDbContext.ExamAnswers.AddAsync(
                new ExamAnswer
                {
                    Identifier     = answerId,
                    OrganizationId = organizationId,
                    CreatedAt      = SystemClock.Instance.GetCurrentInstant(),
                    ExamQuestion   = question,
                },
                cancellationToken
            );
        }
        else
        {
            logger.LogInformation("Updating existing answer {AnswerId}", answerId);
            if (existing.ExamQuestion?.Identifier != questionId)
            {
                logger.LogWarning("Answer {AnswerId} does not belong to question {QuestionId}", answerId, questionId);
                return Unauthorized();
            }
            // No fields in ExamAnswerUpdateDto currently.
        }

        logger.LogDebug("Saving changes for answer {AnswerId}", answerId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully created/updated answer {AnswerId}", answerId);
        return NoContent();
    }

    [HttpGet("all")]
    [ProducesResponseType<ExamAnswerListingDto[]>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        [FromQuery, Required, Range(0, int.MaxValue)] int skip,
        [FromQuery, Required, Range(1, 100)] int take,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("GetAllAsync (Answers) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}, skip: {Skip}, take: {Take}", examId, topicId, questionId, skip, take);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to list answers for question {QuestionId} - no organization ID found", questionId);
            return Unauthorized();
        }

        if (skip < 0 || take < 0)
        {
            logger.LogWarning("Invalid pagination parameters for answers: skip={Skip}, take={Take}", skip, take);
            return BadRequest("Skip and take must be greater than 0");
        }
        if (take > 100)
        {
            logger.LogWarning("Take parameter exceeds maximum for answers: {Take}", take);
            return BadRequest("Take must be less than 100");
        }

        logger.LogDebug("Querying answers for org {OrganizationId}, exam {ExamId}, topic {TopicId}, question {QuestionId} with pagination skip={Skip}, take={Take}", organizationId, examId, topicId, questionId, skip, take);
        var query = examDbContext.ExamAnswers
            .Where(a => a.OrganizationId == organizationId && a.ExamQuestion!.Identifier == questionId && a.ExamQuestion!.ExamTopic!.Identifier == topicId && a.ExamQuestion!.ExamTopic!.Exam!.Identifier == examId)
            .OrderBy(a => a.Id);
        var data = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
        logger.LogInformation("Retrieved {Count} answers for question {QuestionId}", data.Count, questionId);
        return Ok(data.Select(_ => new ExamAnswerListingDto()).ToArray());
    }

    [HttpGet("all/count")]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCountAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("GetAllCountAsync (Answers) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}", examId, topicId, questionId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to get answer count for question {QuestionId} - no organization ID found", questionId);
            return Unauthorized();
        }

        logger.LogDebug("Counting answers for org {OrganizationId}, exam {ExamId}, topic {TopicId}, question {QuestionId}", organizationId, examId, topicId, questionId);
        var count = await examDbContext.ExamAnswers
            .Where(a => a.OrganizationId == organizationId && a.ExamQuestion!.Identifier == questionId && a.ExamQuestion!.ExamTopic!.Identifier == topicId && a.ExamQuestion!.ExamTopic!.Exam!.Identifier == examId)
            .LongCountAsync(cancellationToken);
        logger.LogInformation("Total answer count for question {QuestionId}: {Count}", questionId, count);
        return Ok(count);
    }

    [HttpGet("{answerId:guid}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        [FromRoute] Guid answerId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("DeleteAsync (Answer) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}, answerId: {AnswerId}", examId, topicId, questionId, answerId);

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning("Unauthorized access attempt to delete answer {AnswerId} - no organization ID found", answerId);
            return Unauthorized();
        }

        logger.LogDebug("Querying for answer {AnswerId} including parents for deletion", answerId);
        var existing = await examDbContext.ExamAnswers
            .Include(a => a.ExamQuestion)!.ThenInclude(q => q!.ExamTopic)!.ThenInclude(t => t!.Exam)
            .Where(a => a.Identifier == answerId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            logger.LogInformation("Answer {AnswerId} not found for deletion", answerId);
            return NoContent();
        }
        if (existing.OrganizationId != organizationId)
        {
            logger.LogWarning("Unauthorized deletion attempt: Org {OrganizationId} tried to delete answer {AnswerId} belonging to org {AnswerOrg}", organizationId, answerId, existing.OrganizationId);
            return NoContent();
        }
        if (existing.ExamQuestion?.Identifier != questionId)
        {
            logger.LogWarning("Answer {AnswerId} does not belong to question {QuestionId}", answerId, questionId);
            return NoContent();
        }
        if (existing.ExamQuestion?.ExamTopic?.Identifier != topicId)
        {
            logger.LogWarning("Answer {AnswerId} does not belong to topic {TopicId}", answerId, topicId);
            return NoContent();
        }
        if (existing.ExamQuestion?.ExamTopic?.Exam?.Identifier != examId)
        {
            logger.LogWarning("Answer {AnswerId} does not belong to exam {ExamId}", answerId, examId);
            return NoContent();
        }

        logger.LogInformation("Deleting answer {AnswerId}", answerId);
        examDbContext.ExamAnswers.Remove(existing);
        logger.LogDebug("Saving deletion changes for answer {AnswerId}", answerId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully deleted answer {AnswerId}", answerId);
        return NoContent();
    }
}
