using ENTITIES.Models;
using ENTITIES.ViewModels;
using ENTITIES.ViewModels.Tour;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ITourRepository
    {
         public Task<Tour> GetTourById(long tour_id);
         Task<TourViewModel> GetDetailTourByID(long TourId);
        Task<TourProductDetailModel> GetTourProductById(long id);
        Task<List<ListTourProductViewModel>> GetListTourProduct(string TourType,long pagesize, long pageindex, string StartPoint, string Endpoint);
        Task<List<TourLocationViewModel>> GetLocationById(int tour_type,string s_start_point, string s_end_point);
        Task<List<ListTourProductViewModel>> GetListFavoriteTourProduct(int PageIndex, int PageSize);
        Task<TourDtailFeViewModel> GetDetailTourFeByID(long TourId);
        Task<List<OrderListTourViewModel> > GetListTourByAccountId(long TourId);

    }
}
