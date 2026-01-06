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
        IUpdatedAt,
        IOrganization
{
    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Title { get; set; }

    public partial Guid Identifier { get; set; }
    public partial ICollection<ExamQuestion>? ExamQuestions { get; set; }
    public partial Instant CreatedAt { get; set; }
    public partial Instant UpdatedAt { get; set; }

    public partial int? QuestionAmountToTake { get; set; }


    [ForeignKey(nameof(OrganizationFk))]
    public partial Organization? Organization { get; set; }

    public partial long OrganizationFk { get; set; }

    [ForeignKey(nameof(ExamFk))]
    public partial Exam? Exam { get; set; }

    public partial long ExamFk { get; set; }

    ICollection<ExamQuestion>? IRefersToMany<ExamQuestion>.Entities
    {
        get => ExamQuestions;
        set => ExamQuestions = value;
    }

    Exam? IRefersToOneRequired<Exam, long>.Entity
    {
        get => Exam;
        set => Exam = value;
    }

    long IRefersToOneRequired<Exam, long>.EntityId
    {
        get => ExamFk;
        set => ExamFk = value;
    }
}
