using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.EntityFramework.Mappings
{
    public class ProjectMap : IEntityTypeConfiguration<ProjectbaseinfoEntity>
    {
        public void Configure(EntityTypeBuilder<ProjectbaseinfoEntity> builder)
        {
            builder.ToTable("TWC_PROJECT");
        }
    }
}
