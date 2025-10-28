using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class UserToken : IPrimaryKey<long>, IRefersToOneRequired<User, long>, ICreatedAt
{
    [Key]
    private long _id;

    private string  _accessToken  = string.Empty;
    private string  _refreshToken = string.Empty;
    private Instant _expiresAt;
    private Instant _createdAt;
    private bool    _isRevoked;
    private long    _userId;

    [ForeignKey(nameof(UserId))]
    private User? _user;

    User? IRefersToOneRequired<User, long>.Entity
    {
        get => User;
        set => User = value;
    }

    long IRefersToOneRequired<User, long>.EntityId
    {
        get => UserId;
        set => UserId = value;
    }
}
