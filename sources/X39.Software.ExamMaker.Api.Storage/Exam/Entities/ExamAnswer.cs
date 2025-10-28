using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class ExamAnswer
    : IPrimaryKey<long>, IIdentifier<Guid>, IRefersToOneRequired<ExamQuestion, long>, ICreatedAt, IOrganization
{
    [Key]
    private long _id;

    private string  _answer = string.Empty;
    private string? _reason;
    private bool    _isCorrect;
    private Guid    _identifier = Guid.Empty;
    private Instant _createdAt;

    [ForeignKey(nameof(OrganizationId))]
    private Organization? _organization;

    private long _organizationId;

    [ForeignKey(nameof(ExamQuestionId))]
    private ExamQuestion? _examQuestion;

    private long _examQuestionId;

    ExamQuestion? IRefersToOneRequired<ExamQuestion, long>.Entity
    {
        get => ExamQuestion;
        set => ExamQuestion = value;
    }

    long IRefersToOneRequired<ExamQuestion, long>.EntityId
    {
        get => ExamQuestionId;
        set => ExamQuestionId = value;
    }
}
