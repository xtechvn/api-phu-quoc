﻿using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using ENTITIES.ViewModels;
using ENTITIES.ViewModels.Tour;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace DAL
{
    public class TourProductDAL : GenericService<TourProduct>
    {
        private static DbWorker _DbWorker;

        public TourProductDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);

        }
        public async Task<TourProductDetailModel> GetTourProductById(long id)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[1];
                objParam[0] = new SqlParameter("@TourProductId", id);
                DataTable dt = _DbWorker.GetDataTable(StoreProceduresName.SP_GetDetailTourProductByID, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<TourProductDetailModel>();
                    return data[0];
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourProductById - TourProductDAL: " + ex);
                return null;
            }
        }
        public async Task<List<ListTourProductViewModel>> GetListTourProduct(string TourType, long pagesize, long pageindex, string StartPoint, string Endpoint)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[5];
                objParam[0] = new SqlParameter("@PageIndex", pageindex);
                objParam[1] = new SqlParameter("@PageSize", pagesize);
                if (TourType == null || TourType == "")
                {
                    objParam[2] = new SqlParameter("@TourType", DBNull.Value);

                }
                else
                {
                    objParam[2] = new SqlParameter("@TourType", TourType);

                }
                if (StartPoint == null || StartPoint == "")
                {
                    objParam[3] = new SqlParameter("@StartPoint", DBNull.Value);
                }
                else
                {
                    objParam[3] = new SqlParameter("@StartPoint", StartPoint);
                }
                if (Endpoint == null || Endpoint == "")
                {
                    objParam[4] = new SqlParameter("@Endpoint", DBNull.Value);
                }
                else
                {
                    objParam[4] = new SqlParameter("@Endpoint", Endpoint);
                }

                DataTable dt = _DbWorker.GetDataTable(StoreProceduresName.SP_fe_GetListTourProduct, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<ListTourProductViewModel>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTourProduct - TourProductDAL: " + ex);
                return null;
            }
        }
        public async Task<List<TourLocationViewModel>> GetLocationById(int tour_type, string s_start_point, string s_end_point)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[3];
                objParam[0] = new SqlParameter("@TourType", tour_type);
                objParam[1] = new SqlParameter("@start_point_id", s_start_point);
                objParam[2] = new SqlParameter("@end_point_id", s_end_point);
                DataTable dt = _DbWorker.GetDataTable("Sp_GetListLocationByTourType", objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<TourLocationViewModel>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetLocationById - TourProductDAL: " + ex);
                return null;
            }
        }
        public async Task<List<ListTourProductViewModel>> GetListFavoriteTourProduct(int PageIndex, int PageSize)
        {
            try
            {
                SqlParameter[] objParam = new SqlParameter[2];
                objParam[0] = new SqlParameter("@PageIndex", PageIndex);
                objParam[1] = new SqlParameter("@PageSize", PageSize);
               
                DataTable dt = _DbWorker.GetDataTable(StoreProceduresName.SP_fe_GetListFavoriteTourProduct, objParam);
                if (dt != null && dt.Rows.Count > 0)
                {
                    var data = dt.ToList<ListTourProductViewModel>();
                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListFavoriteTourProduct - TourProductDAL: " + ex);
                return null;
            }
        }
    }
}
