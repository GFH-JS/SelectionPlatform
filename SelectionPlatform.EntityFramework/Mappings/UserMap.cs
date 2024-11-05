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
    public class UserMap : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.Property(user=>user.Account).HasMaxLength(50);
            builder.HasIndex(user=>user.Account).IsUnique();  //设置唯一索引


            ////指定表明，也可以不指定，不指定时使用Dbcontext中的属性明
            //builder.ToTable("T_Books");
            ////可以明确指定数据类型 和精度
            //builder.Property(e => e.Price).HasColumnType("Decimal").HasPrecision(24, 6);
            //builder.Property(e => e.Title).HasMaxLength(50).IsRequired();
            ////指定长度 和必填
            //builder.Property(e => e.AuthorName).HasMaxLength(20).IsRequired();
            ////唯一索引
            //builder.HasIndex(e => e.Title).IsUnique();
        }
    }

   
}
