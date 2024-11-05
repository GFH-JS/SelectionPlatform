using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelectionPlatform.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.EntityFramework.Mappings
{
    public class ParamComparisonMap : IEntityTypeConfiguration<ParamComparison>
    {
        public void Configure(EntityTypeBuilder<ParamComparison> builder)
        {
            builder.ToTable("ParamComparison");
        }
    }
}
