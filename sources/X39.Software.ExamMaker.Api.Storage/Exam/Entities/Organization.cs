using System.ComponentModel.DataAnnotations;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;
[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class Organization : IPrimaryKey<long>, ITitle, IIdentifier<string>, IRefersToMany<User>, ICreatedAt
{
    [Key]
    private long _id;

    private string                                _title      = string.Empty;
    private string                                _identifier = string.Empty;
    private ICollection<User>? _users;
    private Instant                               _createdAt;


    ICollection<User>? IRefersToMany<User>.Entities
    {
        get => Users;
        set => Users = value;
    }
}
