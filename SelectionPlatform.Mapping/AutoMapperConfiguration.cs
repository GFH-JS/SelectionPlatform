using AutoMapper;
using SelectionPlatform.Models.Models;
using SelectionPlatform.Models.ViewModels.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelectionPlatform.Mapping
{
    public class AutoMapperConfiguration:Profile,IAutoMapperIProfile
    {
        public AutoMapperConfiguration()
        {
            CreateMap<UserEntity, UserDto>();
            CreateMap<ProjectInfoEntity, ProjectInfoDto>();  //数据库到前

            CreateMap<CreateUserDto, UserEntity>();  //dto到实体
            CreateMap<UpdateUserDto, UserEntity>();  //dto到实体
            CreateMap<AddProjectDto, ProjectInfoEntity>();  //dto到实体

            CreateMap<ProjectInputDto, ProjectInfoEntity>();  //dto到实体
            CreateMap<ProjectInfoEntity, ProjectInputDto>();  //
            CreateMap<ProjectbaseinfoEntity, ProjectbaseInfoDto>();
            CreateMap<AddProjectbaseInfoDto, ProjectbaseinfoEntity>();  //dto到实体
            CreateMap<UpdateProjectbaseInfoDto, ProjectbaseinfoEntity>();  //dto到实体

            CreateMap<CityEntity, CityResponseDto>();


            //CreateMap<UserEntity, UserDto>()
            //    .ForMember(a => a.Account, o => o.MapFrom(d => d.Account));

            //CreateMap<UserEntity, UserDto>()
            //    .AfterMap((from,to,context) => {
            //        to.Account = from.Account;
            //        to.CreateTime = from.CreateTime;
            //        to.Email = from.Email;
            //    });
        }
    }
}
