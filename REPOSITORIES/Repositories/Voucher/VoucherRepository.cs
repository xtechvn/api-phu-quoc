﻿using DAL;
using Entities.ConfigModels;
using ENTITIES.Models;
using Microsoft.Extensions.Options;
using REPOSITORIES.IRepositories;
using System;
using System.Threading.Tasks;
using Utilities;

namespace REPOSITORIES.Repositories
{
    public class VoucherRepository: IVoucherRepository
    {
        private readonly VoucherDAL _VoucherDAL;

        public VoucherRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var _StrConnection = dataBaseConfig.Value.SqlServer.ConnectionString;
            _VoucherDAL = new VoucherDAL(_StrConnection);
        }


        public async  Task<Voucher> getDetailVoucher(string voucher_name)
        {
            try
            {
                return await _VoucherDAL.FindByVoucherCode(voucher_name);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("[Repository] getDetailVoucher in VoucherRepository" + ex);
                return null;
            }
        }
    }
}
