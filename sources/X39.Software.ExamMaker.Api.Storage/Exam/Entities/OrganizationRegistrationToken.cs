using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using X39.Software.ExamMaker.Api.Storage.Exam.Meta;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Entities;

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
    private long _id;

    [ForeignKey(nameof(OrganizationId))]
    private Organization? _organization;

    private long _organizationId;

    [ForeignKey(nameof(CreatedById))]
    private User? _createdBy;

    private long _createdById;

    [ForeignKey(nameof(UsedById))]
    private User? _usedBy;

    private long? _usedById;

    private Instant  _createdAt;
    private Instant? _usedAt;
    private Instant  _expiresAt;

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
    [Length(340, 340)]
    private string _token = string.Empty;

    [MaxLength(512)]
    string IIdentifier<string>.Identifier
    {
        get => Token;
        set => Token = value;
    }

    User? IRefersToOneOptional<User, long>.Entity
    {
        get => UsedBy;
        set => UsedBy = value;
    }

    long? IRefersToOneOptional<User, long>.EntityId
    {
        get => UsedById;
        set => UsedById = value;
    }

    User? IRefersToOneRequired<User, long>.Entity
    {
        get => CreatedBy;
        set => CreatedBy = value;
    }

    long IRefersToOneRequired<User, long>.EntityId
    {
        get => CreatedById;
        set => CreatedById = value;
    }
}
