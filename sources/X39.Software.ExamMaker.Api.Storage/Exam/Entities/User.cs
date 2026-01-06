using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class User : IPrimaryKey<long>, IOrganization, IRefersToMany<Role>, IRefersToMany<UserToken>
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public partial long Id { get; set; }

    [ForeignKey(nameof(OrganizationFk))]
    public partial Organization? Organization { get; set; }

    public partial long OrganizationFk { get; set; }


    [InverseProperty(nameof(UserToken.User))]
    public partial ICollection<UserToken>? UserTokens { get; set; }

    [InverseProperty(nameof(Role.Users))]
    public partial ICollection<Role>? Roles { get; set; }

    ICollection<UserToken>? IRefersToMany<UserToken>.Entities
    {
        get => UserTokens;
        set => UserTokens = value;
    }

    ICollection<Role>? IRefersToMany<Role>.Entities
    {
        get => Roles;
        set => Roles = value;
    }
}
