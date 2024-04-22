using DAL.Generic;
using DAL.StoreProcedure;
using ENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace DAL.Locations
{
    public class LocationDAL: GenericService<Province>
    {
        private static DbWorker _DbWorker;
        public LocationDAL(string connection) : base(connection)
        {
            _DbWorker = new DbWorker(connection);
        }
      
    }
}
