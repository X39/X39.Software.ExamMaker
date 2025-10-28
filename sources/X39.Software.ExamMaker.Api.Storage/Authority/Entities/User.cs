using System.ComponentModel.DataAnnotations;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Authority.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class User : IPrimaryKey<long>, ITitle, IIdentifier<string>, ICreatedAt
{
    [Key]
    private long _id;

    [MaxLength(255)]
    private string _title = string.Empty;

    [MaxLength(255)]
    private string _eMail = string.Empty;

    private Instant _createdAt;

    private byte[] _passwordHash = [];
    private byte[] _passwordSalt = [];

    [MaxLength(255)]
    string IIdentifier<string>.Identifier
    {
        get => EMail;
        set => EMail = value;
    }
}
