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
        IUpdatedAt,
        IOrganization
{
    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Title { get; set; }

    public partial Guid Identifier { get; set; }
    public partial ICollection<ExamAnswer>? ExamAnswers { get; set; }
    public partial Instant CreatedAt { get; set; }
    public partial Instant UpdatedAt { get; set; }
    public partial EQuestionKind Kind { get; set; }
    public partial int? CorrectAnswersToTake { get; set; }
    public partial int? IncorrectAnswersToTake { get; set; }

    [ForeignKey(nameof(OrganizationFk))]
    public partial Organization? Organization { get; set; }

    public partial long OrganizationFk { get; set; }

    [ForeignKey(nameof(ExamTopicFk))]
    public partial ExamTopic? ExamTopic { get; set; }

    public partial long ExamTopicFk { get; set; }


    ICollection<ExamAnswer>? IRefersToMany<ExamAnswer>.Entities
    {
        get => ExamAnswers;
        set => ExamAnswers = value;
    }

    ExamTopic? IRefersToOneRequired<ExamTopic, long>.Entity
    {
        get => ExamTopic;
        set => ExamTopic = value;
    }

    long IRefersToOneRequired<ExamTopic, long>.EntityId
    {
        get => ExamTopicFk;
        set => ExamTopicFk = value;
    }
}
