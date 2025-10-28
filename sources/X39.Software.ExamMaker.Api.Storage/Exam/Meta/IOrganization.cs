using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Meta;

public interface IOrganization : IRefersToOneRequired<Organization, long>
{
    Organization? Organization { get; set; }
    long OrganizationId { get; set; }

    Organization? IRefersToOneRequired<Organization, long>.Entity
    {
        get => Organization;
        set => Organization = value;
    }

    long IRefersToOneRequired<Organization, long>.EntityId
    {
        get => OrganizationId;
        set => OrganizationId = value;
    }
}
