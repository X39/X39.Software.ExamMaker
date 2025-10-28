using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class User : IPrimaryKey<long>, IOrganization
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    private long _id;

    [ForeignKey(nameof(OrganizationId))]
    private Organization? _organization;
    private long _organizationId;
}
