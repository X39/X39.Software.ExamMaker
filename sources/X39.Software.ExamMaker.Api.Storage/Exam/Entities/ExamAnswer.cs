using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class ExamAnswer
    : IPrimaryKey<long>,
        IIdentifier<Guid>,
        IRefersToOneRequired<ExamQuestion, long>,
        ICreatedAt,
        IUpdatedAt,
        IOrganization
{
    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(4095)]
    public partial string Answer { get; set; }

    [MaxLength(4095)]
    public partial string? Reason { get; set; }

    public partial bool IsCorrect { get; set; }
    public partial Guid Identifier { get; set; }
    public partial Instant CreatedAt { get; set; }
    public partial Instant UpdatedAt { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public partial Organization? Organization { get; set; }

    public partial long OrganizationId { get; set; }

    [ForeignKey(nameof(ExamQuestionId))]
    public partial ExamQuestion? ExamQuestion { get; set; }

    public partial long ExamQuestionId { get; set; }

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
