namespace DucommForge.ViewModels.Agencies;

public interface IAgencyDetailViewModelFactory
{
    AgencyDetailViewModel Create(int agencyId);
}