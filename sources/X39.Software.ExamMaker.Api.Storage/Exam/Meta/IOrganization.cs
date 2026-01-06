using X39.Software.ExamMaker.Api.Storage.Exam.Entities;
using X39.Software.ExamMaker.Api.Storage.Meta;

namespace X39.Software.ExamMaker.Api.Storage.Exam.Meta;

public interface IOrganization : IRefersToOneRequired<Organization, long>
{
    Organization? Organization { get; set; }
    long OrganizationFk { get; set; }

    Organization? IRefersToOneRequired<Organization, long>.Entity
    {
        get => Organization;
        set => Organization = value;
    }

    long IRefersToOneRequired<Organization, long>.EntityId
    {
        get => OrganizationFk;
        set => OrganizationFk = value;
    }
}
