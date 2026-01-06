using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
[Index(nameof(Identifier), IsUnique = true)]
public sealed partial class Exam
    : IPrimaryKey<long>, ITitle, IIdentifier<Guid>, IRefersToMany<ExamTopic>, ICreatedAt, IUpdatedAt, IOrganization
{
    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Title { get; set; }

    public partial Guid Identifier { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(4095)]
    public partial string Preamble { get; set; }

    public partial ICollection<ExamTopic>? ExamTopics { get; set; }
    public partial Instant CreatedAt { get; set; }
    public partial Instant UpdatedAt { get; set; }

    [ForeignKey(nameof(OrganizationFk))]
    public partial Organization? Organization { get; set; }

    public partial long OrganizationFk { get; set; }


    ICollection<ExamTopic>? IRefersToMany<ExamTopic>.Entities
    {
        get => ExamTopics;
        set => ExamTopics = value;
    }
}
