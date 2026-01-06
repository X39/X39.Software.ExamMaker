using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[PublicAPI]
[Index(nameof(Identifier), IsUnique = true)]
[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class Role : IPrimaryKey<long>, ITitle, IIdentifier<string>, IRefersToMany<User>
{
    public Role() { }

    public Role(int id, string identifier, string title, bool isInternal = false)
    {
        Id             = id;
        Identifier     = identifier;
        Title          = title;
        IsInternalRole = isInternal;
    }

    [Key]
    public partial long Id { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Title { get; set; }

    [DefaultValue<string>("")]
    [MaxLength(255)]
    public partial string Identifier { get; set; }

    [InverseProperty(nameof(User.Roles))]
    public partial ICollection<User>? Users { get; set; }

    public partial bool IsInternalRole { get; set; }

    ICollection<User>? IRefersToMany<User>.Entities
    {
        get => Users;
        set => Users = value;
    }
}
