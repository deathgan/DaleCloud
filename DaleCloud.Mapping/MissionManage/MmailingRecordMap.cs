using DaleCloud.Entity.MissionManage;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaleCloud.Mapping.MissionManage
{
  

    public class MmailingRecordMap : EntityTypeConfiguration<MmailingRecordEntity>
    {
        public MmailingRecordMap()
        {
            this.ToTable("MissionManage_MmailingRecord");
            this.HasKey(t => t.F_Id);
        }
    }
}
