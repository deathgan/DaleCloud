/*******************************************************************************
 * Copyright © 2016 DaleCloud.Framework 版权所有
 * Author: DaleCloud
 * Description: DaleCloud快速开发平台
 * Website：
*********************************************************************************/
using DaleCloud.DataBase;
using DaleCloud.Entity.SystemManage;
using System.Collections.Generic;

namespace DaleCloud.Domain.IRepository.SystemManage
{
    public interface IModuleButtonRepository : IRepositoryBase<ModuleButtonEntity>
    {
        void SubmitCloneButton(List<ModuleButtonEntity> entitys);
    }
}
