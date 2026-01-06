using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class UserToken : IPrimaryKey<long>, IRefersToOneRequired<User, long>, ICreatedAt
{
    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    public partial string AccessToken { get; set; }

    [DefaultValue<string>("")]
    [StringLength(88)]
    public partial string RefreshToken { get; set; }

    public partial Instant RefreshTokenExpiresAt { get; set; }
    public partial Instant CreatedAt { get; set; }
    public partial bool IsRevoked { get; set; }

    [ForeignKey(nameof(UserFk))]
    public partial User? User { get; set; }

    public partial long UserFk { get; set; }

    User? IRefersToOneRequired<User, long>.Entity
    {
        get => User;
        set => User = value;
    }

    long IRefersToOneRequired<User, long>.EntityId
    {
        get => UserFk;
        set => UserFk = value;
    }
}
