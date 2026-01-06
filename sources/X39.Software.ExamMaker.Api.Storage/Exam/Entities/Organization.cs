using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class Organization
    : IPrimaryKey<long>, ITitle, IIdentifier<string>, IRefersToMany<User>, ICreatedAt
{
    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Title { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Identifier { get; set; }

    [InverseProperty(nameof(User.Organization))]
    public partial ICollection<User>? Users { get; set; }

    [InverseProperty(nameof(OrganizationRegistrationToken.Organization))]
    public partial ICollection<OrganizationRegistrationToken>? RegistrationTokens { get; set; }

    public partial Instant CreatedAt { get; set; }

    [NotMapped]
    ICollection<User>? IRefersToMany<User>.Entities
    {
        get => Users;
        set => Users = value;
    }
}
