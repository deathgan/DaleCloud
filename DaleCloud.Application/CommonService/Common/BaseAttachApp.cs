/*******************************************************************************
 * Copyright © 2016 DaleCloud.Framework 版权所有
 * Author: DaleCloud
 * Description: DaleCloud快速开发平台
 * Website：
*********************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DaleCloud.Code;
using DaleCloud.Entity.BaseDataManage;
using DaleCloud.Repository.BaseDataManage;
using DaleCloud.DataBase;

namespace DaleCloud.Application.CommonService
{
    public class BaseAttachApp
    {
        private BaseAttachRepository server = new BaseAttachRepository();

        public List<BaseAttachEntity> GetList()
        {
            return server.IQueryable().ToList();
        }

        public List<BaseAttachEntity> GetMapList(string objectId = "")
        {
            var expression = ExtLinq.True<BaseAttachEntity>();
            if (!string.IsNullOrEmpty(objectId))
            {
                expression = expression.And(t => t.T_DocId.Contains(objectId));
            }
            return server.IQueryable(expression).OrderBy(t => t.T_CreateTime).ToList();
        }

        public BaseAttachEntity GetForm(string keyValue)
        {
            return server.FindEntity(keyValue);
        }
       

        private void DeleteForm(string objectId, string fileId="")
        {
            if (string.IsNullOrEmpty(fileId))
            {
                server.Delete(t => t.T_DocId == objectId);
            }else
            {
                server.Delete(t => t.T_DocId == objectId && t.T_Id == fileId);
            }
        }

        public void SubmitForm(string objectId,string filpath)
        {
            if (!string.IsNullOrEmpty(objectId))
            {
                BaseAttachEntity entity = new BaseAttachEntity();
                entity.T_Id = Utils.GuId();
                entity.T_CreateTime = DateTime.Now;
                entity.T_DocId = objectId;
                entity.T_Path = filpath;
                server.Insert(entity);
                
            }
        }

      
    }
    
}
