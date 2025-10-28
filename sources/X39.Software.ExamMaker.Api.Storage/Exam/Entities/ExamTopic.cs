using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class ExamTopic
    : IPrimaryKey<long>,
        ITitle,
        IIdentifier<Guid>,
        IRefersToMany<ExamQuestion>,
        IRefersToOneRequired<Exam, long>,
        ICreatedAt,
        IOrganization
{
    [Key]
    private long _id;

    private string                     _title      = string.Empty;
    private Guid                       _identifier = Guid.Empty;
    private ICollection<ExamQuestion>? _examQuestions;
    private Instant                    _createdAt;

    private int? _questionAmountToTake;


    [ForeignKey(nameof(OrganizationId))]
    private Organization? _organization;

    private long _organizationId;

    ICollection<ExamQuestion>? IRefersToMany<ExamQuestion>.Entities
    {
        get => ExamQuestions;
        set => ExamQuestions = value;
    }

    [ForeignKey(nameof(ExamId))]
    private Exam? _exam;

    private long _examId;

    Exam? IRefersToOneRequired<Exam, long>.Entity
    {
        get => Exam;
        set => Exam = value;
    }

    long IRefersToOneRequired<Exam, long>.EntityId
    {
        get => ExamId;
        set => ExamId = value;
    }
}
