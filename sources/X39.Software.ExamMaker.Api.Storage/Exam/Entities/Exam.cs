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
    : IPrimaryKey<long>, ITitle, IIdentifier<Guid>, IRefersToMany<ExamTopic>, ICreatedAt, IOrganization
{
    [Key]
    private long _id;

    private string                  _title      = string.Empty;
    private Guid                    _identifier = Guid.Empty;
    private string                  _preamble   = string.Empty;
    private ICollection<ExamTopic>? _examTopics;
    private Instant                 _createdAt;

    [ForeignKey(nameof(OrganizationId))]
    private Organization? _organization;

    private long _organizationId;


    ICollection<ExamTopic>? IRefersToMany<ExamTopic>.Entities
    {
        get => ExamTopics;
        set => ExamTopics = value;
    }
}
