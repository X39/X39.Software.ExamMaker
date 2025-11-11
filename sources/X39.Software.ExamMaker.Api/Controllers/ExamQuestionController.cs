using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using X39.Software.ExamMaker.Api.DataTransferObjects.ExamQuestions;
using X39.Software.ExamMaker.Api.Storage.Exam;
using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Shared;

namespace X39.Software.ExamMaker.Api.Controllers;

[ApiController]
[Route("exam/{examId:guid}/topic/{topicId:guid}/question")]
public sealed class ExamQuestionController(ExamDbContext examDbContext, ILogger<ExamQuestionController> logger)
    : ControllerBase
{
    [HttpPut("{questionId:guid}/emplace")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CreateOrUpdateAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        ExamQuestionUpdateDto updateDto,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "CreateOrUpdateAsync (Question) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}",
            examId,
            topicId,
            questionId
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to create/update question {QuestionId} - no organization ID found",
                questionId
            );
            return Unauthorized();
        }

        logger.LogDebug("Loading topic {TopicId} including exam for org {OrganizationId}", topicId, organizationId);
        var topic = await examDbContext.ExamTopics
            .Include(t => t.Exam)
            .Where(t => t.Identifier == topicId)
            .SingleOrDefaultAsync(cancellationToken);
        if (topic is null || topic.Exam?.Identifier != examId)
        {
            logger.LogWarning(
                "Topic {TopicId} under exam {ExamId} not found for question {QuestionId}",
                topicId,
                examId,
                questionId
            );
            return Unauthorized();
        }

        if (topic.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized: Organization {OrganizationId} tried to modify question {QuestionId} under topic {TopicId}",
                organizationId,
                questionId,
                topicId
            );
            return Unauthorized();
        }

        logger.LogDebug("Querying for existing question {QuestionId}", questionId);
        var existing = await examDbContext.ExamQuestions
            .Where(q => q.Identifier == questionId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing is not null && existing.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized: Organization {OrganizationId} tried to modify question {QuestionId} belonging to another org {OtherOrg}",
                organizationId,
                questionId,
                existing.OrganizationId
            );
            return Unauthorized();
        }

        if (existing is null)
        {
            logger.LogInformation("Creating new question {QuestionId} under topic {TopicId}", questionId, topicId);
            var now = SystemClock.Instance.GetCurrentInstant();
            await examDbContext.ExamQuestions.AddAsync(
                new ExamQuestion
                {
                    Identifier             = questionId,
                    OrganizationId         = organizationId,
                    CreatedAt              = now,
                    UpdatedAt              = now,
                    ExamTopic              = topic,
                    Title                  = updateDto.Title ?? string.Empty,
                    CorrectAnswersToTake   = updateDto.CorrectAnswersToTake,
                    IncorrectAnswersToTake = updateDto.IncorrectAnswersToTake,
                    Kind                   = updateDto.Kind,
                },
                cancellationToken
            );
        }
        else
        {
            logger.LogInformation("Updating existing question {QuestionId}", questionId);
            if (existing.ExamTopic?.Identifier != topicId)
            {
                logger.LogWarning("Question {QuestionId} does not belong to topic {TopicId}", questionId, topicId);
                return Unauthorized();
            }

            using (UpdateTimeStampHelper.Create(existing))
            {
                if (updateDto.Title is not null)
                    existing.Title = updateDto.Title.Value;
                if (updateDto.CorrectAnswersToTake is not null)
                    existing.CorrectAnswersToTake = updateDto.CorrectAnswersToTake.Value;
                if (updateDto.IncorrectAnswersToTake is not null)
                    existing.IncorrectAnswersToTake = updateDto.IncorrectAnswersToTake.Value;
                if (updateDto.Kind is not null)
                    existing.Kind = updateDto.Kind.Value;
            }
        }

        logger.LogDebug("Saving changes for question {QuestionId}", questionId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully created/updated question {QuestionId}", questionId);
        return NoContent();
    }

    [HttpGet("all")]
    [ProducesResponseType<ExamQuestionListingDto[]>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromQuery, Required, Range(0, int.MaxValue)] int skip,
        [FromQuery, Required, Range(1, 100)] int take,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "GetAllAsync (Questions) called with examId: {ExamId}, topicId: {TopicId}, skip: {Skip}, take: {Take}",
            examId,
            topicId,
            skip,
            take
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to list questions for topic {TopicId} - no organization ID found",
                topicId
            );
            return Unauthorized();
        }

        if (skip < 0 || take < 0)
        {
            logger.LogWarning("Invalid pagination parameters for questions: skip={Skip}, take={Take}", skip, take);
            return BadRequest("Skip and take must be greater than 0");
        }

        if (take > 100)
        {
            logger.LogWarning("Take parameter exceeds maximum for questions: {Take}", take);
            return BadRequest("Take must be less than 100");
        }

        logger.LogDebug(
            "Querying questions for org {OrganizationId}, exam {ExamId}, topic {TopicId} with pagination skip={Skip}, take={Take}",
            organizationId,
            examId,
            topicId,
            skip,
            take
        );
        var query = examDbContext.ExamQuestions
            .Where(q => q.OrganizationId == organizationId
                        && q.ExamTopic!.Identifier == topicId
                        && q.ExamTopic!.Exam!.Identifier == examId
            )
            .OrderBy(q => q.Title);
        var data = await query.Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        logger.LogInformation("Retrieved {Count} questions for topic {TopicId}", data.Count, topicId);
        return Ok(
            data.Select(e => new ExamQuestionListingDto(
                        e.Identifier,
                        e.Title,
                        e.Kind,
                        e.CorrectAnswersToTake,
                        e.IncorrectAnswersToTake,
                        e.CreatedAt.ToDateTimeOffset()
                    )
                )
                .ToArray()
        );
    }

    [HttpGet("all/count")]
    [ProducesResponseType<long>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCountAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "GetAllCountAsync (Questions) called with examId: {ExamId}, topicId: {TopicId}",
            examId,
            topicId
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to get question count for topic {TopicId} - no organization ID found",
                topicId
            );
            return Unauthorized();
        }

        logger.LogDebug(
            "Counting questions for org {OrganizationId}, exam {ExamId}, topic {TopicId}",
            organizationId,
            examId,
            topicId
        );
        var count = await examDbContext.ExamQuestions
            .Where(q => q.OrganizationId == organizationId
                        && q.ExamTopic!.Identifier == topicId
                        && q.ExamTopic!.Exam!.Identifier == examId
            )
            .LongCountAsync(cancellationToken);
        logger.LogInformation("Total question count for topic {TopicId}: {Count}", topicId, count);
        return Ok(count);
    }

    [HttpGet("{questionId:guid}")]
    [ProducesResponseType<ExamQuestionListingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSingleAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "GetSingleAsync (Question) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}",
            examId,
            topicId,
            questionId
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to get question {QuestionId} - no organization ID found",
                questionId
            );
            return Unauthorized();
        }

        var question = await examDbContext.ExamQuestions
            .Include(q => q.ExamTopic)
            .ThenInclude(t => t!.Exam)
            .Where(q => q.Identifier == questionId)
            .SingleOrDefaultAsync(cancellationToken);

        if (question is null)
        {
            logger.LogInformation("Question {QuestionId} not found", questionId);
            return NotFound();
        }

        if (question.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized access to question {QuestionId} from organization {OrganizationId}",
                questionId,
                organizationId
            );
            return Unauthorized();
        }

        if (question.ExamTopic?.Identifier != topicId || question.ExamTopic?.Exam?.Identifier != examId)
        {
            logger.LogWarning("Question {QuestionId} parent mismatch (topicId/examId)", questionId);
            return Unauthorized();
        }

        return Ok(
            new ExamQuestionListingDto(
                question.Identifier,
                question.Title,
                question.Kind,
                question.CorrectAnswersToTake,
                question.IncorrectAnswersToTake,
                question.CreatedAt.ToDateTimeOffset()
            )
        );
    }

    [HttpGet("{questionId:guid}/delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] Guid examId,
        [FromRoute] Guid topicId,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation(
            "DeleteAsync (Question) called with examId: {ExamId}, topicId: {TopicId}, questionId: {QuestionId}",
            examId,
            topicId,
            questionId
        );

        if (!User.ResolveOrganizationId(out var organizationId))
        {
            logger.LogWarning(
                "Unauthorized access attempt to delete question {QuestionId} - no organization ID found",
                questionId
            );
            return Unauthorized();
        }

        logger.LogDebug("Querying question {QuestionId} including parents for deletion", questionId);
        var existing = await examDbContext.ExamQuestions
            .Include(q => q.ExamTopic)
            .ThenInclude(t => t!.Exam)
            .Where(q => q.Identifier == questionId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            logger.LogInformation("Question {QuestionId} not found for deletion", questionId);
            return NoContent();
        }

        if (existing.OrganizationId != organizationId)
        {
            logger.LogWarning(
                "Unauthorized deletion attempt: Org {OrganizationId} tried to delete question {QuestionId} belonging to org {QuestionOrg}",
                organizationId,
                questionId,
                existing.OrganizationId
            );
            return NoContent();
        }

        if (existing.ExamTopic?.Identifier != topicId)
        {
            logger.LogWarning("Question {QuestionId} does not belong to topic {TopicId}", questionId, topicId);
            return NoContent();
        }

        if (existing.ExamTopic?.Exam?.Identifier != examId)
        {
            logger.LogWarning("Question {QuestionId} does not belong to exam {ExamId}", questionId, examId);
            return NoContent();
        }

        logger.LogInformation("Deleting question {QuestionId}", questionId);
        examDbContext.ExamQuestions.Remove(existing);
        logger.LogDebug("Saving deletion changes for question {QuestionId}", questionId);
        await examDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Successfully deleted question {QuestionId}", questionId);
        return NoContent();
    }
}
