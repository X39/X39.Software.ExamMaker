using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class ExamQuestion
    : IPrimaryKey<long>,
        ITitle,
        IIdentifier<Guid>,
        IRefersToMany<ExamAnswer>,
        IRefersToOneRequired<ExamTopic, long>,
        ICreatedAt,
        IOrganization
{
    [Key]
    private long _id;

    private string                   _title      = string.Empty;
    private Guid                     _identifier = Guid.Empty;
    private ICollection<ExamAnswer>? _examAnswers;
    private Instant                  _createdAt;
    private EQuestionKind            _kind;
    private int?                     _correctAnswersToTake;
    private int?                     _incorrectAnswersToTake;

    [ForeignKey(nameof(OrganizationId))]
    private Organization? _organization;

    private long _organizationId;


    ICollection<ExamAnswer>? IRefersToMany<ExamAnswer>.Entities
    {
        get => ExamAnswers;
        set => ExamAnswers = value;
    }

    [ForeignKey(nameof(ExamTopicId))]
    private ExamTopic? _examTopic;

    private long _examTopicId;

    ExamTopic? IRefersToOneRequired<ExamTopic, long>.Entity
    {
        get => ExamTopic;
        set => ExamTopic = value;
    }

    long IRefersToOneRequired<ExamTopic, long>.EntityId
    {
        get => ExamTopicId;
        set => ExamTopicId = value;
    }
}
