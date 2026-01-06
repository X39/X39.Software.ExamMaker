using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

[PublicAPI]
[NotifyPropertyChanged, NotifyPropertyChanging]
public sealed partial class OrganizationRegistrationToken
    : IPrimaryKey<long>,
        IIdentifier<string>,
        IOrganization,
        IRefersToOneRequired<User, long>,
        IRefersToOneOptional<User, long>,
        ICreatedAt
{
    [Key]
    public partial long Id { get; set; }

    [ForeignKey(nameof(OrganizationFk))]
    public partial Organization? Organization { get; set; }

    public partial long OrganizationFk { get; set; }

    [ForeignKey(nameof(CreatedByFk))]
    public partial User? CreatedBy { get; set; }

    public partial long CreatedByFk { get; set; }

    [ForeignKey(nameof(UsedByFk))]
    public partial User? UsedBy { get; set; }

    public partial long? UsedByFk { get; set; }

    public partial Instant  CreatedAt { get; set; }
    public partial Instant? UsedAt { get; set; }
    public partial Instant  ExpiresAt { get; set; }

    /// <summary>
    /// A private field that is used to store a token associated with the tenant registration link.
    /// The value is constrained to a maximum length of 340 characters and initialized to an
    /// empty string.
    /// </summary>
    /// <remarks>
    /// The 340 length is exactly what is required to fill a 255 sized byte array as base64 encoded string.
    /// <code>
    /// 4 * ( 255 / 3 ) = 340
    /// </code>
    /// </remarks>
    [StringLength(340)]
    [DefaultValue<string>("")]
    public partial string Token { get; set; }

    [NotMapped]
    string IIdentifier<string>.Identifier
    {
        get => Token;
        set => Token = value;
    }

    [NotMapped]
    User? IRefersToOneOptional<User, long>.Entity
    {
        get => UsedBy;
        set => UsedBy = value;
    }

    [NotMapped]
    long? IRefersToOneOptional<User, long>.EntityId
    {
        get => UsedByFk;
        set => UsedByFk = value;
    }

    [NotMapped]
    User? IRefersToOneRequired<User, long>.Entity
    {
        get => CreatedBy;
        set => CreatedBy = value;
    }

    [NotMapped]
    long IRefersToOneRequired<User, long>.EntityId
    {
        get => CreatedByFk;
        set => CreatedByFk = value;
    }
}
