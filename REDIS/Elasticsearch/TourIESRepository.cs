using Caching.Elasticsearch;
using Elasticsearch.Net;
using ENTITIES.ViewModels.Elasticsearch;
using ENTITIES.ViewModels.Tour;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using static Utilities.Contants.CommonConstant;

namespace CACHING.Elasticsearch
{
    public class TourIESRepository : ESRepository<TourESViewModel>
    {

        public TourIESRepository(string Host) : base(Host) { }
        public async Task<List<TourESViewModel>> GetListNational(string txtsearch, string index_name = "national_store")
        {
            List<TourESViewModel> result = new List<TourESViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("national_store");
                var elasticClient = new ElasticClient(connectionSettings);
                if (txtsearch == null) txtsearch = "";
                var search_response = elasticClient.Search<TourESViewModel>(s => s
                          .Index(index_name)
                          .Size(top)
                          .Query(q =>
                            q.Bool(
                                qb => qb.Should(
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.name)
                                    .Query("*" + txtsearch + "*")),
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.code)
                                    .Query("*" + txtsearch + "*"))
                                ))
                           )
                          );
                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<TourESViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<List<ListTourProductViewModel>> GetListTour(string startpoint, string endpoint, int type, int pageindex, int pagesize, string index_name = "tours_store")
        {
            List<ListTourProductViewModel> result = new List<ListTourProductViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("tours_store");
                var elasticClient = new ElasticClient(connectionSettings);
                string typefalse = "false";
                string typetrue = "true";
                if (startpoint != null && startpoint != "" && startpoint != "-1" && endpoint != "-1")
                {
                    var search_response = elasticClient.Search<ListTourProductViewModel>(s => s
                       .Index(index_name)
                       .From(pagesize * (pageindex - 1))
                       .Size(pagesize)
                       .Query(q =>
                         q.Bool(
                             qb => qb.Must(
                               sh => sh.MatchPhrase(m => m
                               .Field(f => f.location_key)
                               .Query("*" + startpoint + "_" + endpoint + "*")),
                                 sh => sh.Term("tourtype", type.ToString()),
                                 sh => sh.Term("status", ((int)CommonStatus.INACTIVE).ToString()),
                                 sh => sh.Term("isdelete", typefalse),
                                 sh => sh.Term("isdisplayweb", typetrue),
                                 sh => sh.Term("isselfdesigned", typefalse)

                               ))
                        ));
                    if (!search_response.IsValid)
                    {
                        return result;
                    }
                    else
                    {
                        result = search_response.Documents as List<ListTourProductViewModel>;
                        return result;
                    }
                }
                if (startpoint != null && startpoint != "" && startpoint == "-1" && endpoint != "-1")
                {
                    var search_response = elasticClient.Search<ListTourProductViewModel>(s => s
                       .Index(index_name)
                        .From(pagesize * (pageindex - 1))
                       .Size(pagesize)
                       .Query(q =>
                         q.Bool(
                            qb => qb.Must(
                               sh => sh.QueryString(m => m
                               .DefaultField(f => f.location_key)
                               .Query("*_" + endpoint)),
                                sh => sh.Term("tourtype", type.ToString()),
                                  sh => sh.Term("status", ((int)CommonStatus.INACTIVE).ToString()),
                                 sh => sh.Term("isdelete", typefalse),
                                 sh => sh.Term("isdisplayweb", typetrue),
                                 sh => sh.Term("isselfdesigned", typefalse)

                             ))

                        )

                       );
                    if (!search_response.IsValid)
                    {
                        return result;
                    }
                    else
                    {
                        result = search_response.Documents as List<ListTourProductViewModel>;
                        return result;
                    }
                }
                if (startpoint == "-1" && endpoint == "-1")
                {
                    var search_response = elasticClient.Search<ListTourProductViewModel>(s => s
                       .Index(index_name)
                       .From(pagesize * (pageindex - 1))
                       .Size(pagesize)
                       .Query(q =>
                         q.Bool(
                            qb => qb.Must(
                               sh => sh.Term("tourtype", type.ToString()),
                                  sh => sh.Term("status", ((int)CommonStatus.INACTIVE).ToString()),
                                 sh => sh.Term("isdelete", typefalse),
                                 sh => sh.Term("isdisplayweb", typetrue),
                                 sh => sh.Term("isselfdesigned", typefalse)

                             ))
                        )
                        );
                    if (!search_response.IsValid)
                    {
                        return result;
                    }
                    else
                    {
                        result = search_response.Documents as List<ListTourProductViewModel>;
                        return result;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTour - TourIESRepository - : " + ex);
                return null;
            }

        }

        public async Task<List<ListTourProductViewModel>> GetListTourId(string TourId, string index_name = "tours_store")
        {
            List<ListTourProductViewModel> result = new List<ListTourProductViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("tours_store");
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<ListTourProductViewModel>(s => s
                       .Index(index_name)
                       .Size(top)
                       .Query(q =>
                         q.Bool(
                             qb => qb.Must(
                               sh => sh.QueryString(m => m
                               .DefaultField(f => f.Id)
                               .Query(TourId)))
                             )

                        )

                       );
                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<ListTourProductViewModel>;
                    return result;
                }



            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTourId - TourIESRepository - : " + ex);
                return null;
            }

        }
        public async Task<List<ListTourProductViewModel>> GetListTour(int type, int pageindex, int pagesize, string index_name = "tours_store")
        {
            List<ListTourProductViewModel> result = new List<ListTourProductViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("tours_store");
                var elasticClient = new ElasticClient(connectionSettings);
                string typefalse = "false";
                string typetrue = "true";
                var search_response = elasticClient.Search<ListTourProductViewModel>(s => s
                        .Index(index_name)
                        .From(pagesize * (pageindex - 1))
                        .Size(pagesize)
                        .Query(q =>
                          q.Bool(
                             qb => qb.Must(
                                 sh => sh.Term("tourtype", type.ToString()),
                                  sh => sh.Term("status", ((int)CommonStatus.INACTIVE).ToString()),
                                 sh => sh.Term("isdelete", typefalse),
                                 sh => sh.Term("isdisplayweb", typetrue),
                                 sh => sh.Term("isselfdesigned", typefalse)
                              ))

                         )
                         .Sort(sort => sort.Field(c => c.updateddate, SortOrder.Descending))
                        );
                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<ListTourProductViewModel>;
                    return result;
                }



            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTour - TourIESRepository - : " + ex);
                return null;
            }

        }
        public async Task<List<TourProductDetailModel>> GetTourDetaiId(int Id, string index_name = "tours_store")
        {
            List<TourProductDetailModel> result = new List<TourProductDetailModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("tours_store");
                var elasticClient = new ElasticClient(connectionSettings);

                var search_response = elasticClient.Search<TourProductDetailModel>(s => s
                        .Index(index_name)
                        .Size(top)
                        .Query(q =>
                          q.Bool(
                             qb => qb.Must(
                                sh => sh.Match(m => m
                                .Field(f => f.Id)
                                .Query(Id.ToString()))
                              ))

                         )

                        );
                if (!search_response.IsValid)
                {
                    return result;
                }
                else
                {
                    result = search_response.Documents as List<TourProductDetailModel>;
                    return result;
                }



            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTourDetaiId - TourIESRepository - : " + ex);
                return null;
            }

        }
    }
}
