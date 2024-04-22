using Elasticsearch.Net;
using Entities.ViewModels;
using ENTITIES.ViewModels.Hotel;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caching.Elasticsearch
{
    public class HotelESRepository : ESRepository<HotelESViewModel>
    {
        public string index_hotel = "hotel_store";

        public HotelESRepository(string Host) : base(Host) { }

        public async Task<List<HotelESViewModel>> GetListProduct(string txtsearch, string index_name = "hotel_store", string Type = "product")
        {
            List<HotelESViewModel> result = new List<HotelESViewModel>();
            try
            {
                int top = 4000;
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex(Type);
                var elasticClient = new ElasticClient(connectionSettings);
                if (txtsearch == null) txtsearch = "";
                var search_response = elasticClient.Search<HotelESViewModel>(s => s
                          .Index(index_name)
                          .From(0)
                          .Size(top)
                          .Query(q => 
                            q.Bool(
                                qb=>qb.Should(
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.name)
                                    .Query("*"+txtsearch+ "*")),
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.city)
                                    .Query("*" + txtsearch + "*")),
                                    sh => sh.QueryString(m => m
                                    .DefaultField(f => f.street)
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
                    result = search_response.Documents as List<HotelESViewModel>;
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public  HotelESViewModel FindByHotelId(string hotel_id)
        {
            try
            {
                var nodes = new Uri[] { new Uri(_ElasticHost) };
                var connectionPool = new StaticConnectionPool(nodes);
                var connectionSettings = new ConnectionSettings(connectionPool).DisableDirectStreaming().DefaultIndex("hotel");
                var elasticClient = new ElasticClient(connectionSettings);

                var searchResponse = elasticClient.Search<HotelESViewModel>(s => s
                    .Index(index_hotel)
                    .Query(q => q.Match(m => m.Field(x=>x.hotelid).Query(hotel_id)))
                );

                var JsonObject = JsonConvert.SerializeObject(searchResponse.Documents);
                var object_result = JsonConvert.DeserializeObject<List<HotelESViewModel>>(JsonObject);
                return object_result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}
